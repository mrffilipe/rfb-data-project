using System.Net;
using System.Text.Json;
using RFBDataProject.API.Common;
using RFBDataProject.Application.Exceptions;
using RFBDataProject.Domain.Exceptions;

namespace RFBDataProject.API.Middlewares;

public sealed class ApplicationExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApplicationExceptionMiddleware> _logger;

    public ApplicationExceptionMiddleware(RequestDelegate next, ILogger<ApplicationExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ApplicationValidationException ex)
        {
            await WriteProblemAsync(context, HttpStatusCode.BadRequest, ApiErrorMessages.APPLICATION_VALIDATION_TITLE, ex.Message);
        }
        catch (ApplicationNotFoundException ex)
        {
            await WriteProblemAsync(context, HttpStatusCode.NotFound, ApiErrorMessages.APPLICATION_NOT_FOUND_TITLE, ex.Message);
        }
        catch (DomainValidationException ex)
        {
            await WriteProblemAsync(context, HttpStatusCode.BadRequest, ApiErrorMessages.DOMAIN_VALIDATION_TITLE, ex.Message);
        }
        catch (DomainBusinessRuleException ex)
        {
            await WriteProblemAsync(context, HttpStatusCode.Conflict, ApiErrorMessages.DOMAIN_BUSINESS_RULE_TITLE, ex.Message);
        }
        catch (DomainNotFoundException ex)
        {
            await WriteProblemAsync(context, HttpStatusCode.NotFound, ApiErrorMessages.NOT_FOUND_TITLE, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception for {Method} {Path}", context.Request.Method, context.Request.Path);
            var isDevelopment = context.RequestServices.GetService<IHostEnvironment>()?.IsDevelopment() == true;
            var detail = isDevelopment ? ex.Message : ApiErrorMessages.UNEXPECTED_ERROR_DETAIL;
            await WriteProblemAsync(context, HttpStatusCode.InternalServerError, ApiErrorMessages.UNHANDLED_SERVER_ERROR_TITLE, detail);
        }
    }

    private static async Task WriteProblemAsync(HttpContext context, HttpStatusCode code, string title, string detail)
    {
        var problem = new Microsoft.AspNetCore.Mvc.ProblemDetails
        {
            Status = (int)code,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };

        context.Response.StatusCode = (int)code;
        context.Response.ContentType = ApiErrorMessages.PROBLEM_JSON_CONTENT_TYPE;
        await context.Response.WriteAsync(JsonSerializer.Serialize(problem), context.RequestAborted);
    }
}
