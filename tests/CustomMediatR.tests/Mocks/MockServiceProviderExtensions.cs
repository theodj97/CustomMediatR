namespace CustomMediatR.tests.Mocks;

public static class MockServiceProviderExtensions
{
    public static object GetRequiredService(this IServiceProvider provider, Type serviceType) => provider.GetService(serviceType) ?? throw new InvalidOperationException($"No service for type '{serviceType}' has been registered.");


    public static IEnumerable<T> GetServices<T>(this IServiceProvider provider) =>
    (IEnumerable<T>?)provider.GetService(typeof(IEnumerable<T>)) ?? [];
}