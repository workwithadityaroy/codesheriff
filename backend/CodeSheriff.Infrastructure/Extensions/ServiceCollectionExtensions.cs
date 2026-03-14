using CodeSheriff.Application.Common.Interfaces;
using CodeSheriff.Application.Common.Options;
using CodeSheriff.Domain.Interfaces;
using CodeSheriff.Infrastructure.Persistence;
using CodeSheriff.Infrastructure.Services;
using CodeSheriff.Infrastructure.Services.AI;
using CodeSheriff.Infrastructure.Services.AI.Providers;
using CodeSheriff.Infrastructure.Services.Email;
using CodeSheriff.Infrastructure.Services.GitHub;
using CodeSheriff.Infrastructure.Services.Queue;
using CodeSheriff.Infrastructure.Workers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

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

        // GitHub
        services.AddScoped<IGitHubService, GitHubService>();
        services.Configure<GitHubOptions>(configuration.GetSection(GitHubOptions.SectionName));
        services.Configure<ClerkOptions>(configuration.GetSection(ClerkOptions.SectionName));

        // Redis queue
        var redisConn = configuration.GetConnectionString("Redis") ?? "localhost:6379";
        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConn));
        services.AddScoped<IReviewQueueService, ReviewQueueService>();
        services.AddHostedService<ReviewBackgroundWorker>();

        // Named HTTP clients
        services.AddHttpClient("github", c =>
        {
            c.BaseAddress = new Uri("https://api.github.com");
            c.DefaultRequestHeaders.Add("User-Agent", "CodeSheriff/1.0");
            c.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");
        });
        services.AddHttpClient("anthropic", c =>
        {
            c.BaseAddress = new Uri("https://api.anthropic.com");
            c.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
        });
        services.AddHttpClient("openai", c =>
        {
            c.BaseAddress = new Uri("https://api.openai.com");
        });

        // AI review — provider abstraction
        services.Configure<AnthropicOptions>(configuration.GetSection(AnthropicOptions.SectionName));
        services.AddScoped<ClaudeAiProvider>();
        services.AddScoped<OpenAiProvider>();
        services.AddScoped<AzureOpenAiProvider>();
        services.AddScoped<IAiReviewService, AiReviewService>();

        // Email (Resend)
        services.Configure<ResendOptions>(configuration.GetSection(ResendOptions.SectionName));
        services.AddHttpClient("resend", c =>
        {
            c.BaseAddress = new Uri("https://api.resend.com/");
        });

        // Clerk backend API (for WeeklyReportWorker user lookup)
        services.AddHttpClient("clerk", c =>
        {
            c.BaseAddress = new Uri("https://api.clerk.com/v1/");
        });

        services.AddScoped<IEmailService, ResendEmailService>();
        services.AddHostedService<WeeklyReportWorker>();

        return services;
    }
}
