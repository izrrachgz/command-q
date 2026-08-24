using Api.Models;
using Common.Models.Response;

namespace Api.Extensions
{
    /// <summary>
    /// Maps internal application responses to stable public API success contracts.
    /// </summary>
    internal static class ApiResponseExtensions
    {
        public static OperationResponse ToOperationResponse(this BasicResponse response)
        {
            return new(response.Message);
        }

        public static PageResponse<T> ToPageResponse<T>(this PagedResponse<T> response)
        {
            return new PageResponse<T>
            {
                Items = response.Collection.ToArray(),
                Total = response.Total,
                Page = response.Page,
                PageSize = response.PageSize,
                TotalPages = response.TotalPages
            };
        }
    }
}
