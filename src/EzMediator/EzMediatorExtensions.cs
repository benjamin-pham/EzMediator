using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace EzMediator;

public static class EzMediatorExtensions
{
    public static IServiceCollection AddCustomMediator(this IServiceCollection services, params Assembly[] assemblies)
    {
        services.AddTransient<IMediator, Mediator>();

        foreach (var assembly in assemblies)
        {
            services.AddRequestHandlers(assembly);
        }        

        return services;
    }

    public static IServiceCollection AddRequestHandlers(this IServiceCollection services, Assembly assembly)
    {
        var handlerType = typeof(IRequestHandler<,>);

        var implementationTypes = assembly.GetTypes()
            .Where(type =>
                !type.IsAbstract &&
                !type.IsInterface &&
                type.GetInterfaces().Any(i =>
                    i.IsGenericType &&
                    i.GetGenericTypeDefinition() == handlerType))
            .ToList();

        var requestHandlerMap = new Dictionary<Type, Type>();

        foreach (var implementation in implementationTypes)
        {
            foreach (var handlerInterface in implementation.GetInterfaces()
                     .Where(i =>
                         i.IsGenericType &&
                         i.GetGenericTypeDefinition() == handlerType))
            {
                var requestType = handlerInterface.GetGenericArguments()[0];

                if (requestHandlerMap.ContainsKey(requestType))
                {
                    throw new InvalidOperationException(
                        $"Request type '{requestType.Name}' có nhiều hơn 1 IRequestHandler: " +
                        $"{requestHandlerMap[requestType].Name} và {implementation.Name}");
                }

                requestHandlerMap[requestType] = implementation;
                services.AddTransient(handlerInterface, implementation);
            }
        }

        return services;
    }
}
