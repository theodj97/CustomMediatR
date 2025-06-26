using System.Collections.Concurrent;
using System.Reflection;
using CustomMediatR.Wrappers;
using Microsoft.Extensions.DependencyInjection;

namespace CustomMediatR;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers de IMediator service.
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddMediatR(this IServiceCollection services)
    {
        services.AddSingleton<IMediator, Mediator>();

        return services;
    }

    public static IServiceCollection AddMediatR(this IServiceCollection services,
                                                params Assembly[] assembliesToScan)
    {
        var handlerWrappers = new ConcurrentDictionary<Type, object>();
        var types = assembliesToScan.SelectMany(a => a.GetTypes());

        foreach (var type in types)
            foreach (var implementedInterface in type.GetInterfaces())
            {
                if (!implementedInterface.IsGenericType) continue;
                if (implementedInterface.GetGenericTypeDefinition() == typeof(IRequestHandler<,>))
                {
                    services.AddTransient(implementedInterface, type);

                    var requestType = implementedInterface.GetGenericArguments()[0];
                    var responseType = implementedInterface.GetGenericArguments()[1];
                    var wrapperType = typeof(RequestHandlerWrapper<,>).MakeGenericType(requestType, responseType);

                    var wrapperInstance = Activator.CreateInstance(wrapperType)
                        ?? throw new InvalidOperationException($"Could not create wrapper for {requestType}");

                    handlerWrappers[requestType] = wrapperInstance;
                }
                else if (implementedInterface.GetGenericTypeDefinition() == typeof(IPipelineBehavior<,>))
                    services.AddTransient(implementedInterface, type);

            }

        services.AddSingleton(handlerWrappers);

        services.AddSingleton<IMediator, Mediator>();

        return services;
    }
}
