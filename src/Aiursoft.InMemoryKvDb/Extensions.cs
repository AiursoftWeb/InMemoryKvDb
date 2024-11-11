using Aiursoft.InMemoryKvDb.AutoCreate;
using Aiursoft.InMemoryKvDb.ManualCreate;
using Microsoft.Extensions.DependencyInjection;

namespace Aiursoft.InMemoryKvDb;

public static class Extensions
{
    public static IServiceCollection AddNamedLruMemoryStore<T, TK>(
        this IServiceCollection services,
        Func<TK, T> onNotFound,
        int maxCachedItemsCount = 1024) where TK : notnull
    {
        services.AddSingleton(_ => new NamedLruMemoryStoreProvider<T, TK>(onNotFound, maxCachedItemsCount));
        return services;
    }
    
    public static IServiceCollection AddNamedLruMemoryStoreManualCreate<T, TK>(
        this IServiceCollection services,
        int maxCachedItemsCount = 1024) where TK : notnull
    {
        services.AddSingleton(_ => new NamedLruMemoryStoreManualCreatedProvider<T, TK>(maxCachedItemsCount));
        return services;
    }
    
    public static IServiceCollection AddLruMemoryStore<T, TK>(
        this IServiceCollection services,
        Func<TK, T> onNotFound,
        int maxCachedItemsCount = 1024) where TK : notnull
    {
        services.AddSingleton(_ => new LruMemoryStore<T, TK>(onNotFound, maxCachedItemsCount));
        return services;
    }
    
    public static IServiceCollection AddLruMemoryStoreManualCreate<T, TK>(
        this IServiceCollection services,
        int maxCachedItemsCount = 1024) where TK : notnull
    {
        services.AddSingleton(_ => new LruMemoryStoreManualCreated<T, TK>(maxCachedItemsCount));
        return services;
    }
}