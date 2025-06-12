using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace CustomMediatR;

public static class IServiceCollectionExtensions
{
    /// <summary>
    /// Registers the MediatR services in the given Assembly.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="assembly"></param>
    /// <returns></returns>
    public static IServiceCollection AddMediatR(this IServiceCollection services, Assembly assembly)
    {
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
}
