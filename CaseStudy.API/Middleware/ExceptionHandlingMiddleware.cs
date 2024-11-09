using FluentValidation.Results;
using CaseStudy.Application.Exceptions;
using CaseStudy.Application.Models;
using CaseStudy.Core.Exceptions;
using Newtonsoft.Json;

namespace CaseStudy.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleException(context, ex);
        }
    }

    private Task HandleException(HttpContext context, Exception ex)
    {
        _logger.LogError(ex.Message);

        var code = StatusCodes.Status500InternalServerError;
        var errors = new List<string> { ex.Message };

        code = ex switch
        {
            NotFoundException => StatusCodes.Status404NotFound,
            ResourceNotFoundException => StatusCodes.Status404NotFound,
            BadRequestException => StatusCodes.Status400BadRequest,
            UnprocessableRequestException => StatusCodes.Status422UnprocessableEntity,
            _ => code
        };
        // if (code == StatusCodes.Status400BadRequest)
        // {
        //     errors.Clear();
        //     JsonConvert.DeserializeObject<List<ValidationFailure>>(ex.Message)
        //         ?.ForEach(e => errors.Add(e.ErrorMessage));
        // }

        var result = JsonConvert.SerializeObject(ApiResult<string>.Failure(errors));

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = code;

        return context.Response.WriteAsync(result);
    }
}