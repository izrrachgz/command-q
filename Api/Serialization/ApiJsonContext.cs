using Api.Models;
using FluentValidation.Results;
using System.Text.Json.Serialization;

namespace Api.Serialization
{
    /// <summary>
    /// Source-generated JSON metadata for API infrastructure serialization.
    /// </summary>
    [JsonSerializable(typeof(ErrorResponse))]
    [JsonSerializable(typeof(ValidationFailure))]
    internal partial class ApiJsonContext : JsonSerializerContext
    {
    }
}
