using Amazon.Lambda.Core;
using Microsoft.Extensions.DependencyInjection;

namespace NBasis.Lambda
{
    public static class Extensions
    {
        public static void AddEventHandler<TEvent, THandler>(this IServiceCollection serviceCollection) where THandler : class, IHandleLambdaEvent<TEvent>
        {
            serviceCollection.AddScoped<IHandleLambdaEvent<TEvent>, THandler>();
        }

        public static void AddRequestHandler<TRequest, TResponse, THandler>(this IServiceCollection serviceCollection) where THandler : class, IHandleLambdaRequest<TRequest,TResponse>
        {
            serviceCollection.AddScoped<IHandleLambdaRequest<TRequest, TResponse>, THandler>();
        }

        public static string GetLambdaAlias(this ILambdaContext ctx)
        {
            if (ctx == null) return null;

            string[] tokens = ctx.InvokedFunctionArn.Split(':');
            if ((tokens != null) && (tokens.Length > 1))
            {
                // if second to last == function name.. then last is alias
                if (tokens[^2].Equals(ctx.FunctionName, StringComparison.OrdinalIgnoreCase))
                    return tokens[^1];
            }
            return ctx.FunctionVersion;
        }
    }
}
