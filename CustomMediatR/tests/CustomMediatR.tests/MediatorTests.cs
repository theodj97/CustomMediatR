using System.Collections.Concurrent;
using System.Reflection;
using System.Threading.Tasks;
using CustomMediatR.tests.Mocks;
using CustomMediatR.Wrappers;
using Microsoft.Extensions.DependencyInjection;

namespace CustomMediatR.tests;

public class MediatorTests
{
    private readonly MockServiceProvider serviceProvider;
    private readonly Mediator mediator;
    private readonly ConcurrentDictionary<Type, object> handlerWrappers;

    public MediatorTests()
    {
        serviceProvider = new MockServiceProvider();
        handlerWrappers = new ConcurrentDictionary<Type, object>();

        RequestHandlerWrapper<MockRequest, MockResponse> requestHandleWrapper = new();
        handlerWrappers[typeof(MockRequest)] = requestHandleWrapper;

        mediator = new Mediator(serviceProvider, handlerWrappers);
    }

    [Fact]
    public async Task Send_ShouldResolveAndCallHandler_WhenHandlerIsRegistered()
    {
        // Arrange
        var request = new MockRequest { Message = "Hello World" };
        var handler = new MockRequestHandler();
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(typeof(MockRequest), typeof(MockResponse));
        serviceProvider.AddService(handlerType, handler);

        // Act
        var response = await mediator.Send(request, CancellationToken.None);

        // Assert
        Assert.True(handler.WasHandled);
        Assert.NotNull(response);
        Assert.Equal("Handled: Hello World", response.Result);
    }

    [Fact]
    public async Task Send_ShouldThrowInvalidOperationException_WhenHandlerIsNotRegistered()
    {
        // Arrange
        handlerWrappers.Remove(typeof(MockRequest), out _);
        var request = new MockRequest { Message = "This will fail" };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            mediator.Send(request, CancellationToken.None)
        );
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(typeof(MockRequest), typeof(MockResponse));
        Assert.Contains($"No handler registered for '{nameof(MockRequest)}'.", exception.Message);
    }

    [Fact]
    public async Task Send_ShouldExecuteSinglePipelineBehavior_WhenRegistered()
    {
        // Arrange
        var request = new MockRequest { Message = "Pipeline Test" };
        var handler = new MockRequestHandler();
        var executionOrder = new List<string>();
        var behavior = new MockPipelineBehavior { Name = "Behavior1", ExecutionOrder = executionOrder };

        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(typeof(MockRequest),
                                                                     typeof(MockResponse));
        serviceProvider.AddService(handlerType, handler);

        var behaviorInterfaceType = typeof(IPipelineBehavior<,>).MakeGenericType(request.GetType(),
                                                                                 typeof(MockResponse));
        serviceProvider.AddEnumerableService(behaviorInterfaceType, behavior);

        // Act
        var response = await mediator.Send(request, CancellationToken.None);

        // Assert
        Assert.True(handler.WasHandled);
        Assert.Equal(2, executionOrder.Count);
        Assert.Equal("Behavior1:Start", executionOrder[0]);
        Assert.Equal("Behavior1:End", executionOrder[1]);
        Assert.Equal("Handled: Pipeline Test:Behavior1", response.Result);
    }

    [Fact]
    public async Task Send_ShouldExecuteMultiplePipelineBehaviorsInCorrectOrder()
    {
        // Arrange
        var request = new MockRequest { Message = "Multi Pipeline" };
        var handler = new MockRequestHandler();
        var executionOrder = new List<string>();
        var behavior1 = new MockPipelineBehavior { Name = "Behavior1", ExecutionOrder = executionOrder };
        var behavior2 = new MockPipelineBehavior { Name = "Behavior2", ExecutionOrder = executionOrder };

        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(typeof(MockRequest), typeof(MockResponse));
        serviceProvider.AddService(handlerType, handler);

        var behaviorInterfaceType = typeof(IPipelineBehavior<,>).MakeGenericType(request.GetType(),
                                                                                 typeof(MockResponse));
        serviceProvider.AddEnumerableService(behaviorInterfaceType, behavior1);
        serviceProvider.AddEnumerableService(behaviorInterfaceType, behavior2);

        // Act
        var response = await mediator.Send(request, CancellationToken.None);

        // Assert
        Assert.Equal(4, executionOrder.Count);
        Assert.Equal("Behavior1:Start", executionOrder[0]);
        Assert.Equal("Behavior2:Start", executionOrder[1]);
        Assert.Equal("Behavior2:End", executionOrder[2]);
        Assert.Equal("Behavior1:End", executionOrder[3]);
        Assert.Equal("Handled: Multi Pipeline:Behavior2:Behavior1", response.Result);
    }


    [Fact]
    public void AddMediatR_ShouldRegisterAllHandlersFromAssembly()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMediatR(Assembly.GetExecutingAssembly());

        // Assert
        var serviceProvider = services.BuildServiceProvider();

        var handlerOne = serviceProvider.GetService<IRequestHandler<MockRequest, MockResponse>>();
        Assert.NotNull(handlerOne);
        Assert.IsType<MockRequestHandler>(handlerOne);
    }

    [Fact]
    public void AddMediatR_ShouldRegisterMediator()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMediatR(Assembly.GetExecutingAssembly());

        // Assert
        var serviceProvider = services.BuildServiceProvider();

        var mediatorService = serviceProvider.GetService<IMediator>();
        Assert.NotNull(mediatorService);
        Assert.IsAssignableFrom<IMediator>(mediatorService);
    }

    [Fact]
    public async Task AddMediatR_ShouldRegisterAllHandlersAndBehavioursFromAssembly_AndSend_ShouldWork()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMediatR(typeof(MediatorTests).Assembly);
        var serviceProvider = services.BuildServiceProvider();
        var testMediator = serviceProvider.GetRequiredService<IMediator>();
        var response = await testMediator.Send(new MockRequest { Message = "Hello World" });


        // Assert
        var handler = serviceProvider.GetService<IRequestHandler<MockRequest, MockResponse>>();
        Assert.NotNull(handler);
        Assert.IsType<MockRequestHandler>(handler);

        var behavior = serviceProvider.GetService<IPipelineBehavior<MockRequest, MockResponse>>();
        Assert.NotNull(behavior);
        Assert.IsType<MockPipelineBehavior>(behavior);

        Assert.NotNull(response);
        Assert.Equal("Handled: Hello World", response.Result);
    }
}
