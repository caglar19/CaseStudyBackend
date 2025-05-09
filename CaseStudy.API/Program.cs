using FluentValidation;
using FluentValidation.AspNetCore;
using CaseStudy.API;
using CaseStudy.API.Middleware;
using CaseStudy.Application;
using CaseStudy.Application.Models.Validators;
using CaseStudy.DataAccess;
using CaseStudy.DataAccess.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddApplication(builder.Configuration);
builder.Services.AddSwagger();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins", builder =>
    {
        builder.AllowAnyOrigin() // Tüm origin'lere izin ver
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});
builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);
builder.WebHost.UseUrls("https://localhost:5001");

var app = builder.Build();

using var scope = app.Services.CreateScope();

app.UseSwagger();
app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "CaseStudy V1"); });

app.UseHttpsRedirection();

app.UseCors(corsPolicyBuilder =>
    corsPolicyBuilder.AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader()
);
app.UseCors("AllowAllOrigins"); // CORS middleware
app.UseDeveloperExceptionPage(); // Geliştirme ortamında hata sayfası

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapControllers();

app.Run();
