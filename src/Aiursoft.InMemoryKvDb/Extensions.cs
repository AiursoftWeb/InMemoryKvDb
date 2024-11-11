using Aiursoft.InMemoryKvDb.AutoCreate;
using Aiursoft.InMemoryKvDb.ManualCreate;
using Microsoft.Extensions.DependencyInjection;

namespace Aiursoft.InMemoryKvDb;

public static class Extensions
{
    public static IServiceCollection AddNamedLruMemoryStore<T>(
        this IServiceCollection services,
        Func<Guid, T> onNotFound,
        int maxCachedItemsCount = 1024)
    {
        services.AddSingleton(_ => new NamedLruMemoryStoreProvider<T>(onNotFound, maxCachedItemsCount));
        return services;
    }
    
    public static IServiceCollection AddNamedLruMemoryStoreManualCreate<T>(
        this IServiceCollection services,
        int maxCachedItemsCount = 1024)
    {
        services.AddSingleton(_ => new NamedLruMemoryStoreManualCreatedProvider<T>(maxCachedItemsCount));
        return services;
    }
    
    public static IServiceCollection AddLruMemoryStore<T>(
        this IServiceCollection services,
        Func<Guid, T> onNotFound,
        int maxCachedItemsCount = 1024)
    {
        services.AddSingleton(_ => new LruMemoryStore<T>(onNotFound, maxCachedItemsCount));
        return services;
    }
    
    public static IServiceCollection AddLruMemoryStoreManualCreate<T>(
        this IServiceCollection services,
        int maxCachedItemsCount = 1024)
    {
        services.AddSingleton(_ => new LruMemoryStoreManualCreated<T>(maxCachedItemsCount));
        return services;
    }
}