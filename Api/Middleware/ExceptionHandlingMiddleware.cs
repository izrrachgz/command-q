using Public;
using Api.Logging;
using Common.Constants;
using FluentValidation;
using System.Text.Json;
using Common.Exceptions;
using FluentValidation.Results;
using Localization.Abstractions;

namespace Api.Middleware
{
    public class ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger, ITextProvider messages) : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (Exception e)
            {
                logger.UnhandledRequestException(e);

                await HandleExceptionAsync(context, e);
            }
        }

        private async Task HandleExceptionAsync(HttpContext httpContext, Exception exception)
        {
            var statusCode = GetStatusCode(exception);

            var response = new
            {
                title = GetTitle(exception, messages),
                status = statusCode,
                exceptionDetail = exception.Message,
                errors = GetErrors(exception)
            };

            httpContext.Response.ContentType = "application/json";

            httpContext.Response.StatusCode = statusCode;

            await httpContext.Response.WriteAsync(JsonSerializer.Serialize(response));
        }

        private static int GetStatusCode(Exception exception) =>
            exception switch
            {
                ValidationException => StatusCodes.Status400BadRequest,
                HttpBadRequestException => StatusCodes.Status400BadRequest,
                HttpNotFoundException => StatusCodes.Status404NotFound,
                HttpForbiddenException => StatusCodes.Status403Forbidden,
                HttpUnauthorizedException => StatusCodes.Status401Unauthorized,
                _ => StatusCodes.Status500InternalServerError
            };

        private string GetTitle(Exception exception, ITextProvider messages) =>
            exception switch
            {
                ApplicationException applicationException => applicationException.Message,
                ValidationException => messages.Get(nameof(Messages.Invalid_Model)),
                HttpBadRequestException => messages.Get(nameof(Messages.Invalid_Model)),
                HttpNotFoundException => messages.Get(nameof(Messages.Not_Found)),
                HttpForbiddenException => messages.Get(nameof(Messages.Forbidden)),
                HttpUnauthorizedException => messages.Get(nameof(Messages.Unauthorized)),
                _ => messages.Get(nameof(Messages.Server_Error))
            };

        private static IEnumerable<ValidationFailure> GetErrors(Exception exception)
        {
            if (exception is ValidationException validationException)
            {
                return validationException.Errors;
            }
            return [];
        }
    }
}
