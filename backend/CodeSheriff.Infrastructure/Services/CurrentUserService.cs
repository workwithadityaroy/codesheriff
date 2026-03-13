using System.Security.Claims;
using CodeSheriff.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace CodeSheriff.Infrastructure.Services;

internal sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string GetClerkUserId()
        => _httpContextAccessor.HttpContext?.User.FindFirstValue("sub")
            ?? throw new InvalidOperationException("No authenticated user in context.");
}
