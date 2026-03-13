using CodeSheriff.Application.Common.Models;

namespace CodeSheriff.Application.Common.Interfaces;

public interface IReviewQueueService
{
    Task EnqueueAsync(ReviewJobMessage message, CancellationToken cancellationToken = default);
}
