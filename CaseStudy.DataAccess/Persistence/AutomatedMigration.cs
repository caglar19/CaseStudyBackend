using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using CaseStudy.Core.Entities;

namespace CaseStudy.DataAccess.Persistence;

public static class AutomatedMigration
{
    public static async Task MigrateAsync(IServiceProvider services)
    {
        var context = services.GetRequiredService<DatabaseContext>();

        if (context.Database.IsNpgsql()) await context.Database.MigrateAsync();

        var userManager = services.GetRequiredService<UserManager<User>>();

        await DatabaseContextSeed.SeedDatabaseAsync(context, userManager);
    }
}