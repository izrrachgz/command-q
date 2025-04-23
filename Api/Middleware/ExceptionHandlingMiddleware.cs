using FluentValidation;
using System.Text.Json;
using Common.Exceptions;
using FluentValidation.Results;

namespace Api.Middleware
{
    public class ExceptionHandlingMiddleware : IMiddleware
    {
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger)
        {
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (Exception e)
            {
                _logger.LogError(exception: e, message: e.Message);

                await HandleExceptionAsync(context, e);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext httpContext, Exception exception)
        {
            var statusCode = GetStatusCode(exception);

            var response = new
            {
                title = GetTitle(exception),
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
                HttpBadResquestException => StatusCodes.Status400BadRequest,
                HttpNotFoundException => StatusCodes.Status404NotFound,
                HttpForbiddenException => StatusCodes.Status403Forbidden,
                HttpUnauthorizedException => StatusCodes.Status401Unauthorized,
                _ => StatusCodes.Status500InternalServerError
            };

        private static string GetTitle(Exception exception) =>
            exception switch
            {
                ApplicationException applicationException => applicationException.Message,
                ValidationException => "Invalid Model",
                HttpBadResquestException => "Invalid Model",
                HttpNotFoundException => "Not Found",
                HttpForbiddenException => "Forbidden",
                HttpUnauthorizedException => "Unauthorized",
                _ => "Server Error"
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
