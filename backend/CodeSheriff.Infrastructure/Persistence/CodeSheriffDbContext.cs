using CodeSheriff.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using DomainRepository = CodeSheriff.Domain.Entities.Repository;

namespace CodeSheriff.Infrastructure.Persistence;

public sealed class CodeSheriffDbContext : DbContext
{
    public CodeSheriffDbContext(DbContextOptions<CodeSheriffDbContext> options)
        : base(options)
    {
    }

    public DbSet<DomainRepository> Repositories => Set<DomainRepository>();
    public DbSet<PullRequest> PullRequests => Set<PullRequest>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<ReviewIssue> ReviewIssues => Set<ReviewIssue>();
    public DbSet<WeeklyReport> WeeklyReports => Set<WeeklyReport>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
