using Amazon.Lambda.Core;
using Microsoft.Extensions.Logging;
using SLogger = Serilog;
using Serilog.Debugging;
using Serilog.Extensions.Logging;
using Serilog.Formatting;

namespace NBasis.Lambda.Serilog
{
    public class LambdaLoggerFactory : ILoggerFactory
    {
        public const string LogEnvironmentVariable = "LOG_ENVIRONMENT";

        public const string LogLevelVariable = "LOG_MINIMUM_LEVEL";

        readonly SerilogLoggerProvider _provider;

        private bool _disposed;

        public LambdaLoggerFactory(SLogger.ILogger logger, bool dispose = false)
        {
            _provider = new SerilogLoggerProvider(logger, dispose);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // force a flush
                    SLogger.Log.CloseAndFlush();

                    _provider.Dispose();
                }

                _disposed = true;
            }
        }

        public Microsoft.Extensions.Logging.ILogger CreateLogger(string categoryName)
        {
            return _provider.CreateLogger(categoryName);
        }

        public void AddProvider(ILoggerProvider provider)
        {
            SelfLog.WriteLine("Cannot add other logger providers {0}", provider);
        }

        public static LambdaLoggerFactory Build(ILambdaContext context, ITextFormatter textFormatter = null)
        {
            ArgumentNullException.ThrowIfNull(context);

            textFormatter ??= new SLogger.Formatting.Json.JsonFormatter();

            var l = new SLogger.LoggerConfiguration();

            // set minimum level
            string logLevel = Environment.GetEnvironmentVariable(LogLevelVariable);
            if (string.IsNullOrWhiteSpace(logLevel)) logLevel = "debug";
            l = logLevel.Trim().ToLower() switch
            {
                "error" => l.MinimumLevel.Error(),
                "info" or "information" => l.MinimumLevel.Information(),
                "warn" or "warning" => l.MinimumLevel.Warning(),
                _ => l.MinimumLevel.Debug(),
            };

            // log environment
            string logEnvVariable = Environment.GetEnvironmentVariable(LogEnvironmentVariable);
            if (!string.IsNullOrWhiteSpace(logEnvVariable))
                l = l.Enrich.WithProperty("environment", logEnvVariable);

            // add process
            l = l.Enrich.WithProperty("process", context.FunctionName);

            // build logger
            var logger = l
                    .WriteTo.LambdaLogger(context.Logger, textFormatter)
                    .CreateLogger();

            return new LambdaLoggerFactory(logger);
        }
    }
}
