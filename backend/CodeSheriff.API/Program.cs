using CodeSheriff.API.Middleware;
using CodeSheriff.Application.Common.Extensions;
using CodeSheriff.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting CodeSheriff API");

    var builder = WebApplication.CreateBuilder(args);

    // Sentry
    builder.WebHost.UseSentry(o =>
    {
        o.Dsn = builder.Configuration["Sentry:Dsn"] ?? string.Empty;
        o.TracesSampleRate = builder.Environment.IsDevelopment() ? 0.0 : 0.1;
        o.Environment = builder.Environment.EnvironmentName;
        o.SendDefaultPii = false;
    });

    // Serilog
    builder.Host.UseSerilog((context, services, configuration) =>
    {
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .WriteTo.Console();
    });

    // Application + Infrastructure layers
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    // Clerk JWT authentication
    var clerkPublishableKey = builder.Configuration["Clerk:PublishableKey"] ?? string.Empty;
    var clerkAuthority = ParseClerkAuthority(clerkPublishableKey);

    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.Authority = clerkAuthority;
            options.MapInboundClaims = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                NameClaimType = "sub"
            };
        });

    builder.Services.AddAuthorization();

    // MVC Controllers
    builder.Services.AddControllers();

    // OpenAPI (built-in .NET 10)
    builder.Services.AddOpenApi(options =>
    {
        options.AddDocumentTransformer((document, context, _) =>
        {
            document.Info.Title = "CodeSheriff API";
            document.Info.Version = "v1";
            document.Info.Description = "AI-Powered Code Review & Tech Debt Analyzer";
            return Task.CompletedTask;
        });
    });

    // Health checks
    builder.Services.AddHealthChecks()
        .AddNpgSql(
            builder.Configuration.GetConnectionString("DefaultConnection")!,
            name: "postgresql",
            tags: ["db", "ready"]);

    // CORS — origins configured per environment via Cors:AllowedOrigins
    var allowedOrigins = (builder.Configuration["Cors:AllowedOrigins"] ?? string.Empty)
        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("Frontend", policy =>
        {
            if (allowedOrigins.Length > 0)
                policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod();
            else
                policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
        });
    });

    // Global exception handler
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

    var app = builder.Build();

    app.UseExceptionHandler();

    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    });

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference(options =>
        {
            options.Title = "CodeSheriff API";
            options.Theme = ScalarTheme.Purple;
        });
    }

    app.UseCors("Frontend");
    app.UseAuthentication();
    if (!app.Environment.IsDevelopment())
        app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();
    app.MapHealthChecks("/health");

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "CodeSheriff API terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

static string ParseClerkAuthority(string publishableKey)
{
    if (string.IsNullOrEmpty(publishableKey)) return string.Empty;
    var parts = publishableKey.Split('_');
    if (parts.Length < 3) return string.Empty;
    var encoded = parts[2];
    encoded = encoded.Replace('-', '+').Replace('_', '/');
    var padding = (4 - encoded.Length % 4) % 4;
    encoded += new string('=', padding);
    var domain = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
    return $"https://{domain.TrimEnd('$')}";
}
