using Api.Middleware;
using Api.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging.Abstractions;

namespace Api.Test.Modules.Common.Middleware
{
    public sealed class CacheMiddlewareTest
    {
        private const int MaxAgeSeconds = 10;

        [Fact]
        public async Task Invoke_Should_Apply_Public_Cache_Headers_Before_Response_Starts()
        {
            var responseFeature = new CapturingResponseFeature();
            var context = CreateContext(responseFeature);
            var middleware = CreateMiddleware(httpContext =>
            {
                httpContext.Response.StatusCode = StatusCodes.Status200OK;
                return responseFeature.Start();
            });

            await middleware.Invoke(context);

            Assert.True(responseFeature.HasStarted);
            Assert.Equal("public, max-age=10", context.Response.Headers.CacheControl);
            Assert.Equal(HeaderNames.AcceptEncoding, context.Response.Headers.Vary);
        }

        [Fact]
        public async Task Invoke_Should_Disable_Caching_For_Bearer_Authenticated_Response()
        {
            var responseFeature = new CapturingResponseFeature();
            var context = CreateContext(responseFeature);
            context.Request.Headers.Authorization = "Bearer access-token";
            var middleware = CreateMiddleware(httpContext =>
            {
                httpContext.Response.StatusCode = StatusCodes.Status200OK;
                return responseFeature.Start();
            });

            await middleware.Invoke(context);

            Assert.Equal("no-store", context.Response.Headers.CacheControl);
            Assert.False(context.Response.Headers.ContainsKey(HeaderNames.Vary));
        }

        [Fact]
        public async Task Invoke_Should_Preserve_Endpoint_Cache_Control()
        {
            var responseFeature = new CapturingResponseFeature();
            var context = CreateContext(responseFeature);
            var middleware = CreateMiddleware(httpContext =>
            {
                httpContext.Response.StatusCode = StatusCodes.Status200OK;
                httpContext.Response.Headers.CacheControl = "private, max-age=30";
                return responseFeature.Start();
            });

            await middleware.Invoke(context);

            Assert.Equal("private, max-age=30", context.Response.Headers.CacheControl);
        }

        private static DefaultHttpContext CreateContext(CapturingResponseFeature responseFeature)
        {
            var context = new DefaultHttpContext();
            context.Features.Set<IHttpResponseFeature>(responseFeature);
            context.Request.Method = HttpMethods.Get;
            return context;
        }

        private static CacheMiddleware CreateMiddleware(RequestDelegate next)
        {
            return new CacheMiddleware(
                next,
                new TestOptionsMonitor<CacheOptions>(new CacheOptions { MaxAgeSeconds = MaxAgeSeconds }),
                NullLogger<CacheMiddleware>.Instance);
        }

        private sealed class TestOptionsMonitor<T>(T value) : IOptionsMonitor<T>
        {
            public T CurrentValue => value;

            public T Get(string? name) => value;

            public IDisposable OnChange(Action<T, string?> listener) => Disposable.Instance;

            private sealed class Disposable : IDisposable
            {
                public static readonly Disposable Instance = new();

                public void Dispose()
                {
                }
            }
        }

        private sealed class CapturingResponseFeature : IHttpResponseFeature
        {
            private readonly Stack<(Func<object, Task> Callback, object State)> onStarting = new();

            public int StatusCode { get; set; } = StatusCodes.Status200OK;
            public string? ReasonPhrase { get; set; }
            public IHeaderDictionary Headers { get; set; } = new HeaderDictionary();
            public Stream Body { get; set; } = Stream.Null;
            public bool HasStarted { get; private set; }

            public void OnCompleted(Func<object, Task> callback, object state)
            {
            }

            public void OnStarting(Func<object, Task> callback, object state)
            {
                onStarting.Push((callback, state));
            }

            public async Task Start()
            {
                while (onStarting.TryPop(out var registration))
                {
                    await registration.Callback(registration.State);
                }

                HasStarted = true;
            }
        }
    }
}
