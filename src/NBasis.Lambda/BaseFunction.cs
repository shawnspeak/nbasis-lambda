using Amazon.Lambda.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace NBasis.Lambda
{
    public abstract class BaseFunction
    {
        protected IConfigurationRoot Configuration { get; private set; }

        protected IServiceProvider ServiceProvider { get; private set; }

        protected ILoggerFactory LoggerFactory { get; private set; }

        protected ILambdaContext Context { get; private set; }

        private bool _hasFirstRun;

        protected BaseFunction()
        {
        }

        protected virtual void AddHandler(IServiceCollection services)
        {
            throw new NotImplementedException("Must implement AddHandler");
        }

        protected virtual void Configure(IConfigurationBuilder configurationBuilder)
        {
        }

        protected virtual void SetupServices(IServiceCollection services)
        {
        }

        protected virtual ILoggerFactory SetupLogging()
        {
            return null;
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
            if (_hasFirstRun) return;

            // setup logging first
            LoggerFactory = SetupLogging();

            // build configuration
            ConfigurationBuilder builder = new();
            builder.AddEnvironmentVariables();
            Configure(builder);
            Configuration = builder.Build();

            // setup services
            ServiceCollection services = new();
            services.AddSingleton(LoggerFactory);
            services.AddSingleton(Configuration);
            services.TryAdd(ServiceDescriptor.Singleton(typeof(ILogger<>), typeof(Logger<>)));
            SetupServices(services);
            AddHandler(services);
            ServiceProvider = services.BuildServiceProvider();

            _hasFirstRun = true;
        }
    }
}
