using CodeSheriff.Domain.Common;
using MediatR;

namespace CodeSheriff.Application.Settings.Commands.TestAiConnection;

public sealed record TestAiConnectionCommand(
    string AiProvider,
    string AiApiKey,
    string AiModel) : IRequest<Result>;
