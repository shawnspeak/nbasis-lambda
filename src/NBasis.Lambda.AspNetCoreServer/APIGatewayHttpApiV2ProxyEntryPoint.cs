using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace NBasis.Lambda.AspNetCoreServer
{
    public class APIGatewayHttpApiV2ProxyEntryPoint<TStartup> :
        Amazon.Lambda.AspNetCoreServer.APIGatewayHttpApiV2ProxyFunction,
        ILambdaRequestFunction<APIGatewayHttpApiV2ProxyRequest, APIGatewayHttpApiV2ProxyResponse>
        where TStartup : class
    {
        protected bool _hasRun = false;

        public ILambdaContext? Context { get; private set; }

        protected ILoggerFactory? LoggerFactory { get; private set; }

        protected virtual ILoggerFactory? SetupLogging(ILambdaContext? context)
        {
            return null;
        }

        /// <summary>
        /// The builder has configuration, logging and Amazon API Gateway already configured. The startup class
        /// needs to be configured in this method using the UseStartup<>() method.
        /// </summary>
        /// <param name="builder"></param>
        protected override void Init(IWebHostBuilder builder)
        {
            builder
                .UseStartup<TStartup>();

            builder.ConfigureServices((services) =>
            {
                if (LoggerFactory != null)
                {
                    services.AddSingleton<ILoggerFactory>(LoggerFactory);
                }
            });
        }

       protected void LogFatalException(Exception ex, string message)
        {
            try // to protect logging cleanup
            {
                if (LoggerFactory != null)
                {
                    var logger = LoggerFactory.CreateLogger(this.GetType());

#pragma warning disable CA2254 // Template should be a static expression
                    logger.LogCritical(ex, message: message);
#pragma warning restore CA2254 // Template should be a static expression

                    // dispose of the logger to trigger any cleanup
                    LoggerFactory.Dispose();
                    LoggerFactory = null;
                }
            }
            catch (Exception)
            {
                // nothing we can do
            }
        }

        protected virtual void SetContext(ILambdaContext ctx)
        {
            // store context
            Context = ctx;

            // handle first run
            if (_hasRun) return;

            // setup logging
            LoggerFactory = SetupLogging(ctx);

            _hasRun = true;
        }

        public override Task<APIGatewayHttpApiV2ProxyResponse> FunctionHandlerAsync(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext lambdaContext)
        {
            try
            {
                // set context 
                SetContext(lambdaContext);
            }
            catch (Exception ex)
            {
                LogFatalException(ex, "Exception setting up function context");
                throw;
            }

            if (request.RawPath == "WarmingLambda")
            {
                var logger = LoggerFactory?.CreateLogger(this.GetType());
                logger?.LogDebug("Warming Lambda");
                return Task.FromResult(new APIGatewayHttpApiV2ProxyResponse { StatusCode = 200 });
            }

            return base.FunctionHandlerAsync(request, lambdaContext);
        }

        public Task<APIGatewayHttpApiV2ProxyResponse> FunctionHandler(APIGatewayHttpApiV2ProxyRequest evt, ILambdaContext ctx)
        {
            return this.FunctionHandlerAsync(evt, ctx);
        }
    }
}
