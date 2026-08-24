using System.Diagnostics;
using System.Globalization;
using System.Net.Mime;
using Api.Constants;
using Localization.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Public;

namespace Api.Extensions
{
    /// <summary>
    /// Creates and enriches RFC-style problem responses exposed by the API.
    /// </summary>
    internal static class ApiProblemDetailsExtensions
    {
        public const string ErrorCodeExtension = "errorCode";
        public const string TraceIdExtension = "traceId";

        public static ObjectResult ApiProblem(
            this ControllerBase controller,
            int statusCode,
            string errorCode,
            string? detail = null)
        {
            var messages = controller.HttpContext.RequestServices.GetRequiredService<ITextProvider>();
            var problem = new ProblemDetails
            {
                Status = statusCode,
                Title = GetTitle(statusCode, messages),
                Detail = detail,
                Instance = controller.HttpContext.Request.Path
            };

            Enrich(problem, controller.HttpContext, errorCode);

            return new ObjectResult(problem)
            {
                StatusCode = statusCode,
                ContentTypes = { MediaTypeNames.Application.ProblemJson, MediaTypeNames.Application.Json }
            };
        }

        public static void Enrich(ProblemDetails problem, HttpContext httpContext, string? errorCode = null)
        {
            int statusCode = problem.Status ?? httpContext.Response.StatusCode;
            string resolvedErrorCode = errorCode ?? GetDefaultErrorCode(statusCode);

            problem.Status = statusCode;
            problem.Type = $"urn:commandq:error:{resolvedErrorCode}";
            problem.Instance ??= httpContext.Request.Path;
            problem.Extensions.TryAdd(ErrorCodeExtension, resolvedErrorCode);
            problem.Extensions.TryAdd(TraceIdExtension, Activity.Current?.Id ?? httpContext.TraceIdentifier);

            if (statusCode is StatusCodes.Status400BadRequest or
                StatusCodes.Status401Unauthorized or
                StatusCodes.Status403Forbidden or
                StatusCodes.Status404NotFound or
                StatusCodes.Status409Conflict or
                StatusCodes.Status500InternalServerError or
                StatusCodes.Status502BadGateway or
                StatusCodes.Status503ServiceUnavailable)
            {
                var messages = httpContext.RequestServices.GetRequiredService<ITextProvider>();
                problem.Title = GetTitle(statusCode, messages);
            }
        }

        public static string GetTitle(int statusCode, ITextProvider messages)
        {
            return statusCode switch
            {
                StatusCodes.Status400BadRequest => messages.Get(nameof(Messages.Invalid_Model)),
                StatusCodes.Status401Unauthorized => messages.Get(nameof(Messages.Unauthorized)),
                StatusCodes.Status403Forbidden => messages.Get(nameof(Messages.Forbidden)),
                StatusCodes.Status404NotFound => messages.Get(nameof(Messages.Not_Found)),
                StatusCodes.Status409Conflict => messages.Get(nameof(Messages.Conflict)),
                StatusCodes.Status502BadGateway => messages.Get(nameof(Messages.Bad_Gateway)),
                StatusCodes.Status503ServiceUnavailable => messages.Get(nameof(Messages.Service_Unavailable)),
                _ => messages.Get(nameof(Messages.Server_Error))
            };
        }

        public static string GetDefaultErrorCode(int statusCode)
        {
            return statusCode switch
            {
                StatusCodes.Status400BadRequest => ApiProblemCodes.BadRequest,
                StatusCodes.Status401Unauthorized => ApiProblemCodes.Unauthorized,
                StatusCodes.Status403Forbidden => ApiProblemCodes.Forbidden,
                StatusCodes.Status404NotFound => ApiProblemCodes.NotFound,
                StatusCodes.Status409Conflict => ApiProblemCodes.Conflict,
                StatusCodes.Status502BadGateway => ApiProblemCodes.UpstreamFailure,
                StatusCodes.Status503ServiceUnavailable => ApiProblemCodes.ServiceUnavailable,
                >= StatusCodes.Status400BadRequest and < StatusCodes.Status500InternalServerError =>
                    $"http.{statusCode.ToString(CultureInfo.InvariantCulture)}",
                _ => ApiProblemCodes.InternalError
            };
        }
    }
}
