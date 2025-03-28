namespace Common.Models.Response
{
    public class DetailResponse<T> : BasicResponse where T : class, new()
    {
        public T? Detail { get; set; }

        public DetailResponse(T model) : base()
        {
            Detail = model;
        }

        public DetailResponse(bool success, string message, T? model) : base(success, message)
        {
            Detail = model;
        }

        public DetailResponse(Exception exception) : base(exception)
        {
            Detail = default;
        }
    }
}
