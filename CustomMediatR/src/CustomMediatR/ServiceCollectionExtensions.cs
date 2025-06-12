using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace CustomMediatR;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the IRequestHandler in the given Assembly, and registers de IMediator service.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="assembly"></param>
    /// <returns></returns>
    public static IServiceCollection AddMediatR(this IServiceCollection services, Assembly assembly)
    {
        services.AddSingleton<IMediator, Mediator>();

        var handlerInterfaceType = typeof(IRequestHandler<,>);

        var handlerTypes = assembly.GetTypes()
            .Where(t => t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerInterfaceType)
                        && !t.IsAbstract && !t.IsInterface);

        foreach (var handlerType in handlerTypes)
        {
            var implementedInterfaces = handlerType.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerInterfaceType);

            foreach (var interfaceType in implementedInterfaces)
                services.AddTransient(interfaceType, handlerType);
        }

        return services;
    }


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
}
