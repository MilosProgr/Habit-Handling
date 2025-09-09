using DevHabit.Api.Data.Configurations;
using DevHabit.Api.Database.Configurations;
using DevHabit.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace DevHabit.Api.Database;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Habit> Habits { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<HabitTag> HabitTags { get; set; }
    public DbSet<Entry> Entries => Set<Entry>();

    public DbSet<User> Users { get; set; }

    public DbSet<GitHubAccessToken> GitHubAccessTokens { get; set; }

    public DbSet<EntryImportJob> EntryImportJobs => Set<EntryImportJob>();



    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Application);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        modelBuilder.ApplyConfiguration(new EntryConfiguration());
        modelBuilder.ApplyConfiguration(new TagConfiguration());
        modelBuilder.ApplyConfiguration(new HabitTagConfiguration());
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new GithubAccessTokenConfiguration());
        modelBuilder.ApplyConfiguration(new EntryImportJobConfiguration());
    }
}
