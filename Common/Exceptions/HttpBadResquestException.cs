namespace Common.Exceptions
{
	public class HttpBadResquestException : Exception
	{
		public HttpBadResquestException(string message)
			: base(message)
		{
		}
	}
}