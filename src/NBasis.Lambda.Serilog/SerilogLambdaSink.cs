using Amazon.Lambda.Core;
using Serilog;
using Serilog.Configuration;
using Serilog.Events;
using Serilog.Formatting;

namespace NBasis.Lambda.Logging
{
    public class SerilogLambdaSink : Serilog.Core.ILogEventSink
    {
        readonly ILambdaLogger _lambdaLogger;

        readonly ITextFormatter _formatter;

        public SerilogLambdaSink(ILambdaLogger lambdaLogger, ITextFormatter formatter)
        {
            _lambdaLogger = lambdaLogger;
            _formatter = formatter;
        }

        public void Emit(LogEvent logEvent)
        {
            var sw = new StringWriter();
            _formatter.Format(logEvent, sw);
            _lambdaLogger.Log(sw.ToString());
        }
    }

    public static class LambdaSinkExtensions
    {
        public static LoggerConfiguration LambdaLogger(this LoggerSinkConfiguration sinkConfig, ILambdaLogger lambdaLogger, ITextFormatter formatter)
        {
            return sinkConfig.Sink(new SerilogLambdaSink(lambdaLogger, formatter));
        }
    }
}
