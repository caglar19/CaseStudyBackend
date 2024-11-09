using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using CaseStudy.Core.Entities;
using CaseStudy.DataAccess.Common;
using CaseStudy.DataAccess.Common.Impl;
using CaseStudy.DataAccess.Persistence;
using CaseStudy.DataAccess.Repositories;
using CaseStudy.DataAccess.Repositories.Impl;

namespace CaseStudy.DataAccess;

public static class DataAccessDependencyInjection
{
    public static IServiceCollection AddDataAccess(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDatabase(configuration);
        services.AddMongo(configuration);

        services.AddIdentity();

        services.AddRepositories();

        return services;
    }

    private static void AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IHolidayRepository, HolidayRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
    }

    private static void AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var databaseConfig = configuration.GetSection("Database").Get<DatabaseConfiguration>();

        //if (databaseConfig.UseInMemoryDatabase)
        //    services.AddDbContext<DatabaseContext>(options =>
        //    {
        //        options.UseInMemoryDatabase("NTierDatabase");
        //        options.ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning));
        //    });
        //else
        services.AddDbContext<DatabaseContext>(options =>
                options.UseMySql(databaseConfig.ConnectionString, ServerVersion.AutoDetect(databaseConfig.ConnectionString)));
    }

    private static void AddIdentity(this IServiceCollection services)
    {
        services.AddIdentity<User, IdentityRole<int>>()
            .AddEntityFrameworkStores<DatabaseContext>()
            .AddDefaultTokenProviders();

        services.Configure<IdentityOptions>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequireUppercase = true;
            options.Password.RequiredLength = 6;
            options.Password.RequiredUniqueChars = 1;

            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;

            options.User.AllowedUserNameCharacters =
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
            options.User.RequireUniqueEmail = true;
        });
    }

    private static void AddMongo(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MongoDbDatabaseSettings>(
            configuration.GetSection(nameof(MongoDbDatabaseSettings)));
        services.AddSingleton<IMongoDbDatabaseSettings>(sp =>
            sp.GetRequiredService<IOptions<MongoDbDatabaseSettings>>().Value);

        services.AddSingleton<IMongoClient, MongoClient>(sp =>
        {
            var settings = sp.GetRequiredService<IMongoDbDatabaseSettings>();
            return new MongoClient(settings.ConnectionString);
        });
    }
}

// TODO move outside?
public class DatabaseConfiguration
{
    public bool UseInMemoryDatabase { get; set; }

    public required string ConnectionString { get; set; }
}