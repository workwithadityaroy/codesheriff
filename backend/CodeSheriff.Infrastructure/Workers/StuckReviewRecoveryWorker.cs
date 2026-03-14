using CodeSheriff.Domain.Enums;
using CodeSheriff.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CodeSheriff.Infrastructure.Workers;

/// <summary>
/// Runs once on startup. Finds any Review records stuck in Pending or Processing
/// state (e.g. because the API was restarted mid-job) and marks them — and their
/// parent PullRequest — as Failed so the user can trigger a fresh re-analysis.
/// </summary>
internal sealed class StuckReviewRecoveryWorker : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<StuckReviewRecoveryWorker> _logger;

    // Only reset reviews that have been stuck for at least this long.
    // Reviews started very recently might still be legitimately in-flight.
    private static readonly TimeSpan StaleThreshold = TimeSpan.FromMinutes(5);

    public StuckReviewRecoveryWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<StuckReviewRecoveryWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<CodeSheriffDbContext>();

        var cutoff = DateTimeOffset.UtcNow - StaleThreshold;

        var stuckReviews = await db.Reviews
            .Where(r =>
                (r.Status == ReviewStatus.Pending || r.Status == ReviewStatus.Processing)
                && r.CreatedAt < cutoff)
            .Include(r => r.PullRequest)
            .ToListAsync(cancellationToken);

        if (stuckReviews.Count == 0)
        {
            _logger.LogInformation("StuckReviewRecoveryWorker: no stuck reviews found.");
            return;
        }

        _logger.LogWarning(
            "StuckReviewRecoveryWorker: found {Count} stuck review(s) — marking as Failed.",
            stuckReviews.Count);

        foreach (var review in stuckReviews)
        {
            review.MarkAsFailed();

            if (review.PullRequest is not null &&
                review.PullRequest.Status is PullRequestStatus.Pending or PullRequestStatus.Reviewing)
            {
                review.PullRequest.MarkAsFailed();
            }
        }

        await db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "StuckReviewRecoveryWorker: reset {Count} stuck review(s) to Failed.",
            stuckReviews.Count);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
