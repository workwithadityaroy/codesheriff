using System.Text.Json;
using CodeSheriff.Application.Common.Models;
using CodeSheriff.Application.Reviews.Commands.AnalyzePullRequest;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace CodeSheriff.Infrastructure.Workers;

internal sealed class ReviewBackgroundWorker : BackgroundService
{
    private const string QueueKey = "codesheriff:review:queue";

    private readonly IConnectionMultiplexer _redis;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ReviewBackgroundWorker> _logger;

    public ReviewBackgroundWorker(
        IConnectionMultiplexer redis,
        IServiceScopeFactory scopeFactory,
        ILogger<ReviewBackgroundWorker> logger)
    {
        _redis = redis;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ReviewBackgroundWorker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var db = _redis.GetDatabase();
                var value = await db.ListLeftPopAsync(QueueKey);

                if (!value.HasValue)
                {
                    await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
                    continue;
                }

                var message = JsonSerializer.Deserialize<ReviewJobMessage>((string)value!);
                if (message is null)
                {
                    _logger.LogWarning("Failed to deserialize queue message.");
                    continue;
                }

                _logger.LogInformation(
                    "Processing review job for PullRequest {PullRequestId}", message.PullRequestId);

                await using var scope = _scopeFactory.CreateAsyncScope();
                var sender = scope.ServiceProvider.GetRequiredService<ISender>();

                var result = await sender.Send(
                    new AnalyzePullRequestCommand(
                        message.PullRequestId,
                        message.InstallationId,
                        message.Owner,
                        message.RepoName,
                        message.GitHubPrNumber),
                    stoppingToken);

                if (!result.IsSuccess)
                {
                    _logger.LogWarning(
                        "Review job failed for PR {PullRequestId}: {Error}",
                        message.PullRequestId, result.Error);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error in ReviewBackgroundWorker.");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }

        _logger.LogInformation("ReviewBackgroundWorker stopped.");
    }
}
