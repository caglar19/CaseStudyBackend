using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CaseStudy.Application.Common.Email;
using CaseStudy.Application.MappingProfiles;
using CaseStudy.Application.Services;
using CaseStudy.Application.Services.DevImpl;
using CaseStudy.Application.Services.Impl;
using CaseStudy.Shared.Services;
using CaseStudy.Shared.Services.Impl;

namespace CaseStudy.Application;

public static class ApplicationDependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IWebHostEnvironment env)
    {
        services.AddServices(env);

        services.RegisterAutoMapper();

        return services;
    }

    private static void AddServices(this IServiceCollection services, IWebHostEnvironment env)
    {
        services.AddScoped<IClaimService, ClaimService>();
        services.AddScoped<ITemplateService, TemplateService>();
        services.AddScoped<IHolidayService, HolidayService>();
        services.AddScoped<IUserService, UserService>();

        if (env.IsDevelopment())
            services.AddScoped<IEmailService, DevEmailService>();
        else
            services.AddScoped<IEmailService, EmailService>();
    }

    private static void RegisterAutoMapper(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(IMappingProfilesMarker));
    }

    public static void AddEmailConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(configuration.GetSection("SmtpSettings").Get<SmtpSettings>());
    }
}