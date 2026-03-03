using CodeSheriff.Application.Common.Interfaces;
using CodeSheriff.Application.Common.Options;
using CodeSheriff.Domain.Interfaces;
using CodeSheriff.Infrastructure.Persistence;
using CodeSheriff.Infrastructure.Services;
using CodeSheriff.Infrastructure.Services.GitHub;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CodeSheriff.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' not found in configuration.");

        services.AddDbContext<CodeSheriffDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(CodeSheriffDbContext).Assembly.FullName);
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorCodesToAdd: null);
            });
        });

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IGitHubService, GitHubService>();
        services.Configure<GitHubOptions>(configuration.GetSection(GitHubOptions.SectionName));
        services.Configure<ClerkOptions>(configuration.GetSection(ClerkOptions.SectionName));

        return services;
    }
}
