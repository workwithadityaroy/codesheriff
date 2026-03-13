using CodeSheriff.Domain.Common;
using MediatR;

namespace CodeSheriff.Application.Reviews.Queries.GetReviewById;

public sealed record GetReviewByIdQuery(Guid ReviewId)
    : IRequest<Result<ReviewDetailDto>>;
