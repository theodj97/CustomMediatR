using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;

[assembly: InternalsVisibleTo("CustomMediatR.tests")]
namespace CustomMediatR.Wrappers;

internal abstract class RequestHandlerWrapper<TResponse>
{
    public abstract Task<TResponse> Handle(IRequest<TResponse> request,
                                           IServiceProvider serviceProvider,
                                           CancellationToken cancellationToken);
}

internal class RequestHandlerWrapper<TRequest, TResponse> : RequestHandlerWrapper<TResponse>
    where TRequest : IRequest<TResponse>
{
    public override Task<TResponse> Handle(IRequest<TResponse> request, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var handler = serviceProvider.GetRequiredService<IRequestHandler<TRequest, TResponse>>();

        var behaviors = serviceProvider.GetServices<IPipelineBehavior<TRequest, TResponse>>();
        if (behaviors.Any()) behaviors = behaviors.Reverse();

        var handlerDelegate = new RequestHandlerDelegate<TResponse>(() => handler.Handle((TRequest)request, cancellationToken));

        var pipeline = behaviors.Aggregate(
            handlerDelegate,
            (next, behavior) => () => behavior.Handle((TRequest)request, next, cancellationToken)
        );

        return pipeline();
    }
}