namespace CustomMediatR.tests.Mocks;

public class MockServiceProvider : IServiceProvider
{
    private readonly Dictionary<Type, object> services = [];
    private readonly Dictionary<Type, List<object>> enumerableServices = [];

    public void AddService<T>(T implementation) where T : class
    {
        services[typeof(T)] = implementation;
    }

    public void AddService(Type serviceType, object implementation)
    {
        services[serviceType] = implementation;
    }

    public void AddEnumerableService<T>(object implementation) where T : class
    {
        if (!enumerableServices.ContainsKey(typeof(T)))
            enumerableServices[typeof(T)] = [];

        enumerableServices[typeof(T)].Add(implementation);
    }

    public void AddEnumerableService(Type serviceType, object implementation)
    {
        if (!enumerableServices.TryGetValue(serviceType, out List<object>? value))
        {
            value = [];
            enumerableServices[serviceType] = value;
        }

        value.Add(implementation);
    }

    public object? GetService(Type serviceType)
    {
        if (serviceType.IsGenericType && serviceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
        {
            var itemType = serviceType.GetGenericArguments()[0];
            if (enumerableServices.TryGetValue(itemType, out var services))
                return services;

            return Enumerable.Empty<object>();
        }

        if (services.TryGetValue(serviceType, out var service))
            return service;

        return null;
    }
}
