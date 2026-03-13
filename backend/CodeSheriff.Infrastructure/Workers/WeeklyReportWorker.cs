using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using CodeSheriff.Application.Reports.Commands.SendWeeklyReport;
using CodeSheriff.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CodeSheriff.Infrastructure.Workers;

internal sealed class WeeklyReportWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<WeeklyReportWorker> _logger;

    public WeeklyReportWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<WeeklyReportWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("WeeklyReportWorker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var delay = GetDelayUntilNextRun();
            _logger.LogInformation(
                "Next weekly report run in {Hours:F1}h ({RunAt:u})",
                delay.TotalHours, DateTimeOffset.UtcNow.Add(delay));

            try
            {
                await Task.Delay(delay, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            await SendAllReportsAsync(stoppingToken);
        }

        _logger.LogInformation("WeeklyReportWorker stopped.");
    }

    private async Task SendAllReportsAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Sending weekly reports...");

        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            var httpFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();

            var repos = await unitOfWork.Repositories.GetActiveRepositoriesAsync(stoppingToken);
            var userIds = repos.Select(r => r.ClerkUserId).Where(id => !string.IsNullOrEmpty(id)).Distinct().ToList();

            _logger.LogInformation("Sending weekly reports to {Count} users.", userIds.Count);

            var clerkClient = httpFactory.CreateClient("clerk");

            foreach (var clerkUserId in userIds)
            {
                try
                {
                    var (email, name) = await GetClerkUserInfoAsync(clerkClient, clerkUserId, stoppingToken);

                    if (string.IsNullOrEmpty(email))
                    {
                        _logger.LogWarning("No email for Clerk user {ClerkUserId} — skipping.", clerkUserId);
                        continue;
                    }

                    await sender.Send(
                        new SendWeeklyReportCommand(clerkUserId, email, name),
                        stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send weekly report for user {ClerkUserId}.", clerkUserId);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in WeeklyReportWorker.SendAllReportsAsync.");
        }
    }

    private static async Task<(string Email, string Name)> GetClerkUserInfoAsync(
        HttpClient clerkClient,
        string clerkUserId,
        CancellationToken ct)
    {
        var response = await clerkClient.GetAsync($"users/{clerkUserId}", ct);
        if (!response.IsSuccessStatusCode) return (string.Empty, string.Empty);

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };
        var body = await response.Content.ReadFromJsonAsync<ClerkUserResponse>(options, ct);
        if (body is null) return (string.Empty, string.Empty);

        var email = body.EmailAddresses?.FirstOrDefault(e => e.Id == body.PrimaryEmailAddressId)?.EmailAddress
                 ?? body.EmailAddresses?.FirstOrDefault()?.EmailAddress
                 ?? string.Empty;

        var name = string.IsNullOrWhiteSpace($"{body.FirstName} {body.LastName}".Trim())
            ? email
            : $"{body.FirstName} {body.LastName}".Trim();

        return (email, name);
    }

    // Next Monday 9am UTC (or 1 minute from now in dev if today is Monday and past 9am)
    private static TimeSpan GetDelayUntilNextRun()
    {
        var now = DateTimeOffset.UtcNow;
        var next = now.Date;

        // Find next Monday
        int daysUntilMonday = ((int)DayOfWeek.Monday - (int)now.DayOfWeek + 7) % 7;
        if (daysUntilMonday == 0 && now.Hour >= 9) daysUntilMonday = 7; // Already past 9am Monday
        next = next.AddDays(daysUntilMonday).AddHours(9);

        var delay = next - now;
        return delay <= TimeSpan.Zero ? TimeSpan.FromMinutes(1) : delay;
    }

    private sealed record ClerkUserResponse(
        string? FirstName,
        string? LastName,
        string? PrimaryEmailAddressId,
        List<ClerkEmailAddress>? EmailAddresses);

    private sealed record ClerkEmailAddress(string Id, string EmailAddress);
}
