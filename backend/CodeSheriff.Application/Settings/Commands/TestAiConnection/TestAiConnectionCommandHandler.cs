using CodeSheriff.Application.Common.Interfaces;
using CodeSheriff.Domain.Common;
using MediatR;

namespace CodeSheriff.Application.Settings.Commands.TestAiConnection;

internal sealed class TestAiConnectionCommandHandler
    : IRequestHandler<TestAiConnectionCommand, Result>
{
    private readonly IAiReviewService _aiReviewService;

    public TestAiConnectionCommandHandler(IAiReviewService aiReviewService)
    {
        _aiReviewService = aiReviewService;
    }

    public async Task<Result> Handle(TestAiConnectionCommand request, CancellationToken cancellationToken)
        => await _aiReviewService.TestConnectionAsync(
            request.AiProvider, request.AiApiKey, request.AiModel, cancellationToken);
}
