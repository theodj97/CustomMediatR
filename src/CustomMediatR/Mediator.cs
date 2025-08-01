using System.Collections;
using Microsoft.Extensions.DependencyInjection;

namespace CustomMediatR;

public class Mediator(IServiceProvider serviceProvider) : IMediator
{
    private readonly IServiceProvider serviceProvider = serviceProvider;
    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        var requestType = request.GetType();

        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse))
            ?? throw new InvalidOperationException($"No IRequestHandler<{requestType}, {typeof(TResponse)}> type found found.");
        var handler = serviceProvider.GetRequiredService(handlerType)
            ?? throw new InvalidOperationException($"No IRequestHandler<{requestType}, {typeof(TResponse)}> found in ServiceProvider.");

        var behaviorInterfaceType = typeof(IPipelineBehavior<,>).MakeGenericType(requestType, typeof(TResponse));
        var enumerableBehaviorType = typeof(IEnumerable<>).MakeGenericType(behaviorInterfaceType);

        var behaviorsObject = serviceProvider.GetService(enumerableBehaviorType);

        var behaviors = (behaviorsObject as IEnumerable ?? Enumerable.Empty<object>()).Cast<object>().Reverse().ToList();

        var handlerDelegate = new RequestHandlerDelegate<TResponse>(() =>
            (Task<TResponse>)handlerType.GetMethod(nameof(IRequestHandler<IRequest<object>, object>.Handle))!
                .Invoke(handler, [request, cancellationToken])!
        );

        var pipeline = behaviors.Aggregate(
            handlerDelegate,
            (next, pipelineBehavior) =>
                () => (Task<TResponse>)behaviorInterfaceType.GetMethod(nameof(IPipelineBehavior<IRequest<object>, object>.Handle))!
                    .Invoke(pipelineBehavior, [request, next, cancellationToken])!
        );

        return pipeline();
    }
}
