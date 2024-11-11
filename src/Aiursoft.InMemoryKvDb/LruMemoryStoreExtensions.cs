using Microsoft.Extensions.DependencyInjection;

namespace Aiursoft.InMemoryKvDb;

public static class LruMemoryStoreExtensions
{
    public static IServiceCollection AddNamedLruMemoryStore<T>(
        this IServiceCollection services,
        Func<Guid, T> onNotFound,
        int maxCachedItemsCount = 1024)
    {
        services.AddSingleton(_ => new NamedLruMemoryStoreProvider<T>(onNotFound, maxCachedItemsCount));
        return services;
    }
}