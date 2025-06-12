using System.Reflection;
using CustomMediatR.tests.Mocks;
using Microsoft.Extensions.DependencyInjection;

namespace CustomMediatR.tests;

public class MediatorTests
{
    private readonly MockServiceProvider serviceProvider;
    private readonly Mediator mediator;
    public MediatorTests()
    {
        serviceProvider = new MockServiceProvider();
        mediator = new Mediator(serviceProvider);
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
        var request = new MockRequest { Message = "This will fail" };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            mediator.Send(request, CancellationToken.None)
        );
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(typeof(MockRequest), typeof(MockResponse));
        Assert.Contains($"No service for type '{handlerType}' has been registered.", exception.Message);
    }

    [Fact]
    public async Task Send_ShouldExecuteSinglePipelineBehavior_WhenRegistered()
    {
        // Arrange
        var request = new MockRequest { Message = "Pipeline Test" };
        var handler = new MockRequestHandler();
        var executionOrder = new List<string>();
        var behavior = new MockPipelineBehavior("Behavior1", executionOrder);

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
        var behavior1 = new MockPipelineBehavior("Behavior1", executionOrder);
        var behavior2 = new MockPipelineBehavior("Behavior2", executionOrder);

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
}
