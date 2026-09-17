using Api.Constants;
using Api.Extensions;
using Api.Logging;
using Api.Middleware.Exceptions;
using Common.Exceptions;
using Email.Client.Exceptions;
using FluentValidation;
using Localization.Abstractions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Public;

namespace Api.Middleware
{
    /// <summary>
    /// Converts unhandled request exceptions into the API problem-details contract.
    /// </summary>
    internal sealed class ApiExceptionHandler(
        IProblemDetailsService problemDetailsService,
        IHostEnvironment environment,
        ILogger<ApiExceptionHandler> logger) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            logger.UnhandledRequestException(exception);

            var messages = httpContext.RequestServices.GetRequiredService<ITextProvider>();
            int statusCode = GetStatusCode(exception);
            string errorCode = GetErrorCode(exception);
            ProblemDetails problem = CreateProblem(exception, statusCode, messages);

            ApiProblemDetailsExtensions.Enrich(problem, httpContext, errorCode);
            httpContext.Response.StatusCode = statusCode;

            return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = problem,
                Exception = exception
            });
        }

        private ProblemDetails CreateProblem(Exception exception, int statusCode, ITextProvider messages)
        {
            if (exception is ValidationException validationException)
            {
                var errors = validationException.Errors
                    .GroupBy(error => string.IsNullOrWhiteSpace(error.PropertyName) ? "request" : error.PropertyName)
                    .ToDictionary(
                        group => group.Key,
                        group => group.Select(error => error.ErrorMessage).Distinct().ToArray());

                return new ValidationProblemDetails(errors)
                {
                    Status = statusCode,
                    Title = messages.Get(nameof(Messages.Invalid_Model))
                };
            }

            return new ProblemDetails
            {
                Status = statusCode,
                Title = ApiProblemDetailsExtensions.GetTitle(statusCode, messages),
                Detail = GetDetail(exception)
            };
        }

        private string GetDetail(Exception exception)
        {
            if (environment.IsDevelopment() || IsSafeClientException(exception))
                return exception.Message;

            return ApiMessages.ProductionExceptionDetail;
        }

        private static int GetStatusCode(Exception exception) =>
            exception switch
            {
                ValidationException or HttpBadRequestException => StatusCodes.Status400BadRequest,
                HttpUnauthorizedException => StatusCodes.Status401Unauthorized,
                HttpForbiddenException or NotValidApiClientException => StatusCodes.Status403Forbidden,
                HttpNotFoundException => StatusCodes.Status404NotFound,
                EmailApiClientException => StatusCodes.Status502BadGateway,
                _ => StatusCodes.Status500InternalServerError
            };

        private static string GetErrorCode(Exception exception) =>
            exception switch
            {
                ValidationException => ApiProblemCodes.ValidationFailed,
                HttpBadRequestException => ApiProblemCodes.BadRequest,
                HttpUnauthorizedException => ApiProblemCodes.Unauthorized,
                HttpForbiddenException => ApiProblemCodes.Forbidden,
                NotValidApiClientException => ApiProblemCodes.ApiClientInvalid,
                HttpNotFoundException => ApiProblemCodes.NotFound,
                EmailApiClientException => ApiProblemCodes.UpstreamFailure,
                _ => ApiProblemCodes.InternalError
            };

        private static bool IsSafeClientException(Exception exception)
        {
            return exception is ValidationException or HttpBadRequestException or HttpUnauthorizedException or
                HttpForbiddenException or NotValidApiClientException or HttpNotFoundException;
        }
    }
}
