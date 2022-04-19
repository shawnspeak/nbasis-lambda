using Amazon.Lambda.Core;
using Microsoft.Extensions.DependencyInjection;

namespace NBasis.Lambda
{
    public interface ILambdaEventFunction<TEvent>
    {
        Task FunctionHandler(TEvent evt, ILambdaContext ctx);
    }

    public abstract class BaseEventFunction<TEvent, THandler> : BaseFunction, ILambdaEventFunction<TEvent>
        where THandler : class, IHandleLambdaEvent<TEvent>
    {
        public async Task FunctionHandler(TEvent evt, ILambdaContext ctx)
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
                // get a scope
                var scopeFactory = ServiceProvider.GetRequiredService<IServiceScopeFactory>();
                using var scope = scopeFactory.CreateScope();

                // resolve the handler
                var eventHandler = scope.ServiceProvider.GetService<IHandleLambdaEvent<TEvent>>();

                // handle event
                await eventHandler.HandleEventAsync(evt, ctx);
            }
            catch (Exception ex)
            {
                LogFatalException(ex, ex.Message);
                throw;
            }
        }

        protected override void AddHandler(IServiceCollection services)
        {
            services.AddEventHandler<TEvent, THandler>();
        }
    }
}
