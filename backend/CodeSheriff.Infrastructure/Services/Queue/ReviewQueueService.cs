using System.Text.Json;
using CodeSheriff.Application.Common.Interfaces;
using CodeSheriff.Application.Common.Models;
using StackExchange.Redis;

namespace CodeSheriff.Infrastructure.Services.Queue;

internal sealed class ReviewQueueService : IReviewQueueService
{
    private const string QueueKey = "codesheriff:review:queue";

    private readonly IConnectionMultiplexer _redis;

    public ReviewQueueService(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task EnqueueAsync(ReviewJobMessage message, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        var json = JsonSerializer.Serialize(message);
        await db.ListRightPushAsync(QueueKey, json);
    }
}
