using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace EzMediator;

public static class EzMediatorExtensions
{
    public static IServiceCollection AddMediator(this IServiceCollection services, params Assembly[] assemblies)
    {
        var handlerInterfaceType = typeof(IRequestHandler<,>);

        Type[] types = assemblies.SelectMany(a => a.GetTypes()).ToArray();

        foreach (var type in types)
        {
            if (type.IsAbstract || type.IsInterface) continue;

            var implementedInterfaces = type.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerInterfaceType);

            foreach (var @interface in implementedInterfaces)
            {
                services.AddScoped(@interface, type);
            }
        }

        services.AddScoped<IMediator>(serviceProvider =>
        {
            return new Mediator(serviceProvider, types);
        });

        return services;
    }

    public static IServiceCollection AddRequestHandlers(this IServiceCollection services, params Assembly[] assemblies)
    {
        Type[] types = assemblies.SelectMany(a => a.GetTypes()).ToArray();

        foreach (var type in types)
        {
            if (type.IsAbstract || type.IsInterface)
                continue;

            // Đăng ký IRequestHandler<,>
            var interfaces = type.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>));

            foreach (var handlerInterface in interfaces)
            {
                services.AddScoped(handlerInterface, type);
            }

            //// Đăng ký IRequestPipeline<,>
            //var pipelines = type.GetInterfaces()
            //    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IPipelineBehavior<,>));

            //foreach (var pipelineInterface in pipelines)
            //{
            //    services.AddScoped(pipelineInterface, type);
            //}
        }

        services.AddScoped<IMediator>(serviceProvider =>
        {
            return new Mediator(serviceProvider, types);
        });

        return services;
    }
}
