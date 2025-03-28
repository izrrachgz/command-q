namespace Common.Models.Response
{
    public class BasicResponse
    {
        public bool Success { get; set; }

        public string Message { get; set; }

        public BasicResponse()
        {
            Success = false;
            Message = string.Empty;
        }

        public BasicResponse(bool success, string message)
        {
            Success = success;
            Message = message;
        }

        public BasicResponse(Exception exception)
        {
            Success = false;
            Message = exception.Message;
        }
    }
}
