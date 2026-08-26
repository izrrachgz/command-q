using Cqrs.Abstractions;
using Cqrs.DependencyInjection;
using Cqrs.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace Cqrs.Test.Dispatching
{
    public sealed class RequestDispatcherTest
    {
        [Fact]
        public async Task Send_Should_Invoke_The_Registered_Handler()
        {
            var services = new ServiceCollection();
            services.AddRequestHandler<EchoRequest, string, EchoRequestHandler>();

            await using ServiceProvider provider = services.BuildServiceProvider();
            await using AsyncServiceScope scope = provider.CreateAsyncScope();
            var dispatcher = scope.ServiceProvider.GetRequiredService<IRequestDispatcher>();

            string response = await dispatcher.Send(new EchoRequest("hello"), TestContext.Current.CancellationToken);

            Assert.Equal("hello", response);
        }

        [Fact]
        public async Task Send_Should_Execute_Behaviors_In_Registration_Order()
        {
            var calls = new List<string>();
            var services = new ServiceCollection();
            services.AddSingleton(calls);
            services.AddRequestHandler<EchoRequest, string, EchoRequestHandler>();
            services.AddPipelineBehavior<EchoRequest, string, FirstBehavior>();
            services.AddPipelineBehavior<EchoRequest, string, SecondBehavior>();

            await using ServiceProvider provider = services.BuildServiceProvider();
            await using AsyncServiceScope scope = provider.CreateAsyncScope();
            var dispatcher = scope.ServiceProvider.GetRequiredService<IRequestDispatcher>();

            await dispatcher.Send(new EchoRequest("hello"), TestContext.Current.CancellationToken);

            Assert.Equal(["first:before", "second:before", "handler", "second:after", "first:after"], calls);
        }

        [Fact]
        public async Task Send_Should_Resolve_Handlers_From_The_Current_Scope()
        {
            var services = new ServiceCollection();
            services.AddScoped<ScopeMarker>();
            services.AddRequestHandler<ScopeRequest, Guid, ScopeRequestHandler>();

            await using ServiceProvider provider = services.BuildServiceProvider();
            Guid firstScopeValue;

            await using (AsyncServiceScope firstScope = provider.CreateAsyncScope())
            {
                var dispatcher = firstScope.ServiceProvider.GetRequiredService<IRequestDispatcher>();
                firstScopeValue = await dispatcher.Send(new ScopeRequest(), TestContext.Current.CancellationToken);
                Guid repeatedValue = await dispatcher.Send(new ScopeRequest(), TestContext.Current.CancellationToken);
                Assert.Equal(firstScopeValue, repeatedValue);
            }

            await using AsyncServiceScope secondScope = provider.CreateAsyncScope();
            var secondDispatcher = secondScope.ServiceProvider.GetRequiredService<IRequestDispatcher>();
            Guid secondScopeValue = await secondDispatcher.Send(new ScopeRequest(), TestContext.Current.CancellationToken);

            Assert.NotEqual(firstScopeValue, secondScopeValue);
        }

        [Fact]
        public async Task Send_Should_Create_A_Transient_Handler_For_Each_Dispatch()
        {
            var services = new ServiceCollection();
            services.AddRequestHandler<HandlerInstanceRequest, Guid, HandlerInstanceRequestHandler>();

            await using ServiceProvider provider = services.BuildServiceProvider();
            await using AsyncServiceScope scope = provider.CreateAsyncScope();
            var dispatcher = scope.ServiceProvider.GetRequiredService<IRequestDispatcher>();

            Guid firstHandler = await dispatcher.Send(new HandlerInstanceRequest(), TestContext.Current.CancellationToken);
            Guid secondHandler = await dispatcher.Send(new HandlerInstanceRequest(), TestContext.Current.CancellationToken);

            Assert.NotEqual(firstHandler, secondHandler);
        }

        [Fact]
        public async Task Send_Should_Forward_The_Cancellation_Token()
        {
            var services = new ServiceCollection();
            services.AddRequestHandler<CancellationRequest, bool, CancellationRequestHandler>();

            await using ServiceProvider provider = services.BuildServiceProvider();
            await using AsyncServiceScope scope = provider.CreateAsyncScope();
            var dispatcher = scope.ServiceProvider.GetRequiredService<IRequestDispatcher>();
            using var cancellation = new CancellationTokenSource();
            await cancellation.CancelAsync();

            bool wasCancelled = await dispatcher.Send(new CancellationRequest(), cancellation.Token);

            Assert.True(wasCancelled);
        }

        [Fact]
        public async Task Send_Should_Support_Commands_Without_A_Result_Payload()
        {
            var services = new ServiceCollection();
            services.AddRequestHandler<CompletionCommand, Unit, CompletionCommandHandler>();

            await using ServiceProvider provider = services.BuildServiceProvider();
            await using AsyncServiceScope scope = provider.CreateAsyncScope();
            var dispatcher = scope.ServiceProvider.GetRequiredService<IRequestDispatcher>();

            Unit result = await dispatcher.Send(new CompletionCommand(), TestContext.Current.CancellationToken);

            Assert.Equal(Unit.Value, result);
        }

        [Fact]
        public async Task Send_Should_Reject_An_Unregistered_Request()
        {
            var services = new ServiceCollection();
            services.AddCqrs();

            await using ServiceProvider provider = services.BuildServiceProvider();
            await using AsyncServiceScope scope = provider.CreateAsyncScope();
            var dispatcher = scope.ServiceProvider.GetRequiredService<IRequestDispatcher>();

            MissingRequestHandlerException exception = await Assert.ThrowsAsync<MissingRequestHandlerException>(
                () => dispatcher.Send(new MissingRequest(), TestContext.Current.CancellationToken));

            Assert.Equal(typeof(MissingRequest), exception.RequestType);
        }

        [Fact]
        public void Registration_Should_Reject_A_Duplicate_Handler()
        {
            var services = new ServiceCollection();
            services.AddRequestHandler<EchoRequest, string, EchoRequestHandler>();

            DuplicateRequestHandlerException exception = Assert.Throws<DuplicateRequestHandlerException>(
                () => services.AddRequestHandler<EchoRequest, string, AlternativeEchoRequestHandler>());

            Assert.Equal(typeof(EchoRequest), exception.RequestType);
        }

        private sealed record EchoRequest(string Value) : IRequest<string>;

        private sealed class EchoRequestHandler(List<string>? calls = null) : IRequestHandler<EchoRequest, string>
        {
            public Task<string> Handle(EchoRequest request, CancellationToken cancellationToken)
            {
                calls?.Add("handler");
                return Task.FromResult(request.Value);
            }
        }

        private sealed class AlternativeEchoRequestHandler : IRequestHandler<EchoRequest, string>
        {
            public Task<string> Handle(EchoRequest request, CancellationToken cancellationToken)
            {
                return Task.FromResult(request.Value);
            }
        }

        private sealed class FirstBehavior(List<string> calls) : IPipelineBehavior<EchoRequest, string>
        {
            public async Task<string> Handle(
                EchoRequest request,
                RequestHandlerDelegate<EchoRequest, string> next,
                CancellationToken cancellationToken)
            {
                calls.Add("first:before");
                string result = await next(request, cancellationToken);
                calls.Add("first:after");
                return result;
            }
        }

        private sealed class SecondBehavior(List<string> calls) : IPipelineBehavior<EchoRequest, string>
        {
            public async Task<string> Handle(
                EchoRequest request,
                RequestHandlerDelegate<EchoRequest, string> next,
                CancellationToken cancellationToken)
            {
                calls.Add("second:before");
                string result = await next(request, cancellationToken);
                calls.Add("second:after");
                return result;
            }
        }

        private sealed record ScopeRequest : IRequest<Guid>;

        private sealed class ScopeRequestHandler(ScopeMarker marker) : IRequestHandler<ScopeRequest, Guid>
        {
            public Task<Guid> Handle(ScopeRequest request, CancellationToken cancellationToken)
            {
                return Task.FromResult(marker.Id);
            }
        }

        private sealed class ScopeMarker
        {
            public Guid Id { get; } = Guid.NewGuid();
        }

        private sealed record HandlerInstanceRequest : IRequest<Guid>;

        private sealed class HandlerInstanceRequestHandler : IRequestHandler<HandlerInstanceRequest, Guid>
        {
            private readonly Guid _instanceId = Guid.NewGuid();

            public Task<Guid> Handle(HandlerInstanceRequest request, CancellationToken cancellationToken)
            {
                return Task.FromResult(_instanceId);
            }
        }

        private sealed record CancellationRequest : IRequest<bool>;

        private sealed class CancellationRequestHandler : IRequestHandler<CancellationRequest, bool>
        {
            public Task<bool> Handle(CancellationRequest request, CancellationToken cancellationToken)
            {
                return Task.FromResult(cancellationToken.IsCancellationRequested);
            }
        }

        private sealed record MissingRequest : IRequest<string>;

        private sealed record CompletionCommand : ICommand;

        private sealed class CompletionCommandHandler : ICommandHandler<CompletionCommand>
        {
            public Task<Unit> Handle(CompletionCommand request, CancellationToken cancellationToken)
            {
                return Task.FromResult(Unit.Value);
            }
        }
    }
}
