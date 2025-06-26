using System.Collections.Concurrent;
using CustomMediatR.Wrappers;

namespace CustomMediatR;

public class Mediator(IServiceProvider serviceProvider,
                      ConcurrentDictionary<Type, object> handlerWrappers) : IMediator
{
    private readonly IServiceProvider serviceProvider = serviceProvider;
    private readonly ConcurrentDictionary<Type, object> handlerWrappers = handlerWrappers;

    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        var requestType = request.GetType();

        if (handlerWrappers.TryGetValue(requestType, out var wrapperObject) is false)
            throw new InvalidOperationException($"No handler registered for '{requestType.Name}'.");

        var wrapper = (RequestHandlerWrapper<TResponse>)wrapperObject;

        return wrapper.Handle(request, serviceProvider, cancellationToken);
    }
}
