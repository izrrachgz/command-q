namespace Api.Middleware
{
    public sealed class SecurityHeadersMiddleware(RequestDelegate next)
    {
        public async Task Invoke(HttpContext context)
        {
            context.Response.OnStarting(() =>
            {
                var headers = context.Response.Headers;

                headers.TryAdd("X-Content-Type-Options", "nosniff");
                headers.TryAdd("X-Frame-Options", "DENY");
                headers.TryAdd("Referrer-Policy", "no-referrer");
                headers.TryAdd("X-Permitted-Cross-Domain-Policies", "none");

                if (context.Request.IsHttps)
                {
                    headers.TryAdd("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
                }

                return Task.CompletedTask;
            });

            await next(context);
        }
    }
}
