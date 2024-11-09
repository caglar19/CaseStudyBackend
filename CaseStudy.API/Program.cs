using FluentValidation;
using FluentValidation.AspNetCore;
using CaseStudy.API;
using CaseStudy.API.Filters;
using CaseStudy.API.Middleware;
using CaseStudy.Application;
using CaseStudy.Application.Models.Validators;
using CaseStudy.DataAccess;
using CaseStudy.DataAccess.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(
    config => config.Filters.Add(typeof(ValidateModelAttribute))
);

// builder.Services.AddFluentValidationAutoValidation();
// Bu kütüphane hala geliştirme aşamasında olduğu için FluentValidation'ın kendi auto validation özelliğini kullanmıyoruz.
// ASP.NET Auto Validation async validationları desteklemediği için FluentValidation'ın auto validation özelliğini kullanamıyoruz.
// Bunun yerine kendimiz manuel olarak çağıracağız.

builder.Services.AddValidatorsFromAssemblyContaining(typeof(IValidationsMarker));

builder.Services.AddSwagger();

builder.Services.AddDataAccess(builder.Configuration)
    .AddApplication(builder.Environment);

builder.Services.AddJwt(builder.Configuration); 

builder.Services.AddEmailConfiguration(builder.Configuration);

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("Admin", policy => policy.RequireRole("Admin"))
    .AddPolicy("Customer", policy => policy.RequireRole("Customer"));

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

using var scope = app.Services.CreateScope();

await AutomatedMigration.MigrateAsync(scope.ServiceProvider);

app.UseSwagger();
app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "Study.IO V1"); });

app.UseHttpsRedirection();

app.UseCors(corsPolicyBuilder =>
    corsPolicyBuilder.AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader()
);

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.UseMiddleware<PerformanceMiddleware>();

app.UseMiddleware<TransactionMiddleware>();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapControllers();

app.Run();