using Amazon.Lambda.Core;
using Microsoft.Extensions.DependencyInjection;

namespace NBasis.Lambda
{
    public interface ILambdaRequestFunction<TRequest, TResponse>
    {
        Task<TResponse> FunctionHandler(TRequest evt, ILambdaContext ctx);
    }

    public abstract class BaseRequestFunction<TRequest, TResponse, THandler> : BaseFunction, ILambdaRequestFunction<TRequest, TResponse>
        where THandler : class, IHandleLambdaRequest<TRequest, TResponse>
    {
        public async Task<TResponse> FunctionHandler(TRequest evt, ILambdaContext ctx)
        {
            try
            {
                // set context 
                SetContext(ctx);
            }
            catch (Exception ex)
            {
                LogFatalException(ex, "Exception setting up function context");
                throw;
            }

            try
            {
                var scopeFactory = ServiceProvider.GetRequiredService<IServiceScopeFactory>();
                using var scope = scopeFactory.CreateScope();

                // resolve the handler
                var eventHandler = scope.ServiceProvider.GetService<IHandleLambdaRequest<TRequest, TResponse>>();

                // handle event
                return await eventHandler.HandleRequestAsync(evt, ctx);
            }
            catch (Exception ex)
            {
                LogFatalException(ex, ex.Message);
                throw;
            }
        }

        protected override void AddHandler(IServiceCollection services)
        {
            services.AddRequestHandler<TRequest, TResponse, THandler>();
        }
    }
}
