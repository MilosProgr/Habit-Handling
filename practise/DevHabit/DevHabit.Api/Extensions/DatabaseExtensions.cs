using Dapper;
using DevHabit.Api.Common.Auth;
using DevHabit.Api.Database;
using DevHabit.Api.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DevHabit.Api.Extensions;


public static class DatabaseExtensions
{
    public static async Task ApplyMigrationsAsync(this WebApplication app)
    {
        using IServiceScope scope = app.Services.CreateScope();
        await using ApplicationDbContext dbContext =
            scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        try
        {
            await dbContext.Database.MigrateAsync();
            app.Logger.LogInformation("Application database migrations applied successfully.");

            // 👉 Dodatno pravljenje cache tabele
            var dataSource = app.Services.GetRequiredService<Npgsql.NpgsqlDataSource>();
            await using var connection = await dataSource.OpenConnectionAsync();

            await connection.ExecuteAsync(
                """
            CREATE UNLOGGED TABLE IF NOT EXISTS cache (
                id SERIAL PRIMARY KEY,
                key TEXT UNIQUE NOT NULL,
                value JSONB,
                created_at_utc TIMESTAMP DEFAULT CURRENT_TIMESTAMP);

            CREATE INDEX IF NOT EXISTS idx_cache_key ON cache (key) INCLUDE (value);
            """);

            app.Logger.LogInformation("Cache table ensured successfully.");
        }
        catch (Npgsql.PostgresException ex) when (ex.SqlState == "57P03")
        {
            // Pravilno prosleđivanje exception-a
            app.Logger.LogWarning(ex, "Database is starting up, retry later.");
            throw;
        }
        catch (Exception e)
        {
            // Pravilno prosleđivanje exception-a
            app.Logger.LogError(e, "An error occurred while applying database migrations.");
            throw;
        }

    }

    public static async Task SendInitialDataAsync(this WebApplication app)
    {
        using IServiceScope scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        try
        {
            if (!await dbContext.Habits.AnyAsync())
            {
                await dbContext.Habits.AddAsync(new Habit {
                    Id = Habit.CreateNewId(),
                    UserId = "system",
                    Name = "First habit",
                    Type = HabitType.None,
                    Frequency = new Frequency
                    {
                        Type = FrequencyType.Weekly,
                        TimesPerPeriod = 1
                    },
                    Target = new Target
                    {
                        Value = 1,
                        Unit = "unit" // npr. "times" ili neki default
                    },
                    Status = HabitStatus.None,
                    IsArchived = false,
                    CreatedAtUtc = DateTime.UtcNow,
                    HabitTags = new List<HabitTag>(),
                    Tags = new List<Tag>(),
                    _automationSource = Habit.AutomationSource.None
                });
                await dbContext.SaveChangesAsync();
            }

            app.Logger.LogInformation("Initial data seeded successfully.");
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "An error occurred while seeding initial data.");
            throw;
        }
    }

}

//public static class DatabaseExtensions
//{
//    public static async Task ApplyMigrationsAsync(this WebApplication app)
//    {
//        using IServiceScope scope = app.Services.CreateScope();
//        await using ApplicationDbContext dbContext = 
//            scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
//        await using ApplicationIdentityDbContext identitydbContext =
//            scope.ServiceProvider.GetRequiredService<ApplicationIdentityDbContext>();

//        try
//        {
//            await dbContext.Database.MigrateAsync();
//            app.Logger.LogInformation("Application database migrations applied successfully.");

//            await identitydbContext.Database.MigrateAsync();

//            app.Logger.LogInformation("Identity database migrations applied successfully.");
//        }
//        catch (Exception e)
//        {
//            app.Logger.LogError(e, "An error occurred while applying database migrations.");
//            throw;
//        }
//    }

//    public static async Task SendInitialDataAsync(this WebApplication app)
//    {
//        using IServiceScope scope = app.Services.CreateScope();
//        RoleManager<IdentityRole> roleManager = 
//            scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

//        try
//        {
//            if(!await roleManager.RoleExistsAsync(Roles.Member))
//            {
//                await roleManager.CreateAsync(new IdentityRole(Roles.Member));
//            }
//            if(!await roleManager.RoleExistsAsync(Roles.Admin))
//            {
//                await roleManager.CreateAsync(new IdentityRole(Roles.Admin));
//            }

//            app.Logger.LogInformation("Uspesno kreirana rola");
//        }

//        catch (Exception ex) 
//        {
//            app.Logger.LogError(ex,"Uspesno kreirana rola");
//            throw;

//        }
//    }
//}
