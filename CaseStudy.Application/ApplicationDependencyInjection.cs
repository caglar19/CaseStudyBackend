using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CaseStudy.Application.Common.Email;
using CaseStudy.Application.MappingProfiles;
using CaseStudy.Application.Services.Impl;
using CaseStudy.Shared.Services;
using CaseStudy.Shared.Services.Impl;
using CaseStudy.Application.Interfaces;
using CaseStudy.Application.Services;

namespace CaseStudy.Application;

public static class ApplicationDependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddServices();

        services.RegisterAutoMapper();

        return services;
    }

    private static void AddServices(this IServiceCollection services)
    {
        services.AddScoped<IHolidayService, HolidayService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IBayTahminService, BayTahminService>();
        services.AddScoped<IFootballDataService, FootballDataService>();
        services.AddScoped<IRouletteService, RouletteService>();
        services.AddScoped<MLPredictionService>();
    }
    private static void RegisterAutoMapper(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(IMappingProfilesMarker));
    }
}