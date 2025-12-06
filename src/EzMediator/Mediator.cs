using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace EzMediator;

public class Mediator(IServiceProvider serviceProvider) : IMediator
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    private static readonly ConcurrentDictionary<Type, Delegate> _handlerDelegateCache = new();

    public async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        var requestType = request.GetType();
        var responseType = typeof(TResponse);

        var behaviors = _serviceProvider.GetServices<IPipelineBehavior<IRequest<TResponse>, TResponse>>().ToList();
        var handlerInterface = typeof(IRequestHandler<,>).MakeGenericType(requestType, responseType);
        var handlerInstance = _serviceProvider.GetRequiredService(handlerInterface);

        var handleDelegate = (Func<object, object, CancellationToken, Task<TResponse>>)
            _handlerDelegateCache.GetOrAdd(handlerInterface, _ =>
            {
                var handlerParam = Expression.Parameter(typeof(object), "handler");
                var requestParam = Expression.Parameter(typeof(object), "request");
                var tokenParam = Expression.Parameter(typeof(CancellationToken), "token");

                var castedHandler = Expression.Convert(handlerParam, handlerInterface);
                var castedRequest = Expression.Convert(requestParam, requestType);

                var handleMethod = handlerInterface.GetMethod("Handle")!;
                var callHandle = Expression.Call(castedHandler, handleMethod, castedRequest, tokenParam);

                var lambda = Expression.Lambda<Func<object, object, CancellationToken, Task<TResponse>>>(
                    callHandle, handlerParam, requestParam, tokenParam);

                return lambda.Compile();
            });

        Func<Task<TResponse>> handlerFunc = () => handleDelegate(handlerInstance, request, cancellationToken);

        foreach (var behavior in behaviors.AsEnumerable().Reverse())
        {
            var next = handlerFunc;
            handlerFunc = () => behavior.HandleAsync(request, next, cancellationToken);
        }

        return await handlerFunc();
    }
}