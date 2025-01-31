using Amazon.Lambda.Core;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace NBasis.Lambda
{
    public abstract class MultiEventFunction : BaseFunction
    {
        readonly Dictionary<Type, Func<string, bool>> _detectorTable = [];

        readonly Amazon.Lambda.Core.ILambdaSerializer _serializer;

        private MethodInfo _deserializerMethod = null;
        private bool _hasCatchAll = false;

        public MultiEventFunction(Amazon.Lambda.Core.ILambdaSerializer serializer)
        {
            _serializer = serializer;
        }

        [LambdaSerializer(typeof(MultiEventSerializer))]
        public async Task FunctionHandler(MultiEvent multiEvent, ILambdaContext ctx)
        {
            // set context
            SetContext(ctx);

            // get a scope
            var scopeFactory = ServiceProvider.GetRequiredService<IServiceScopeFactory>();
            using var scope = scopeFactory.CreateScope();

            // detect the event
            var evtType = multiEvent.DetectType(_detectorTable);
            if (evtType == null)
            {
                if (!_hasCatchAll)
                    throw new ApplicationException("Could not detect event");
                evtType = typeof(string);
            }

            // resolve the handler
            var handlerType = typeof(IHandleLambdaEvent<>).MakeGenericType(evtType);
            var eventHandler = scope.ServiceProvider.GetService(handlerType);
            if (eventHandler == null)
                throw new ApplicationException("Could not resolve the event handler");

            // deserialize the event
            object evt;
            if (evtType == typeof(string))
            {
                evt = multiEvent.GetString();
            }
            else
            {
                var genericMethod = _deserializerMethod.MakeGenericMethod(evtType);
                evt = genericMethod.Invoke(_serializer, [multiEvent.GetStream()]);
            }

            // handle event
            var handleMethod = handlerType.GetTypeInfo().GetDeclaredMethod("HandleEventAsync");
            await (Task)handleMethod.Invoke(eventHandler, [evt, ctx]);
        }

        protected void AddMultiEventHandler<TEvent, THandler>(IServiceCollection services) where THandler : class, IHandleLambdaEvent<TEvent>
        {
            // add handler
            services.AddScoped<IHandleLambdaEvent<TEvent>, THandler>();

            // add appropriate detector
            var type = typeof(TEvent);
            if (_detectorTable.ContainsKey(type))
                throw new ArgumentException("Already contains handler for type");

            // get deserialize method if needed
            if (_deserializerMethod == null)
                _deserializerMethod = _serializer.GetType().GetTypeInfo().GetDeclaredMethod("Deserialize");

            if (type.FullName.Equals("Amazon.Lambda.KinesisEvents.KinesisEvent", StringComparison.Ordinal))
            {
                _detectorTable[type] = (input) =>
                {
                    return input.Contains("\"aws:kinesis\"");
                };
            }
            else if (type.FullName.Equals("Amazon.Lambda.SNSEvents.SNSEvent", StringComparison.Ordinal))
            {
                _detectorTable[type] = (input) =>
                {
                    return input.Contains("\"aws:sns\"");
                };
            }
            else if (type.FullName.Equals("Amazon.Lambda.DynamoDBEvents.DynamoDBEvent", StringComparison.Ordinal))
            {
                _detectorTable[type] = (input) =>
                {
                    return input.Contains("\"aws:dynamodb\"");
                };
            }
            else if (type.FullName.Equals("Amazon.Lambda.S3Events.S3Event", StringComparison.Ordinal))
            {
                _detectorTable[type] = (input) =>
                {
                    return input.Contains("\"aws:s3\"");
                };
            }
            else if (type == typeof(string))
            {
                // set a catch all
                _hasCatchAll = true;
            }
            else
            {
                throw new ArgumentOutOfRangeException("TEvent", "Unsupported multi-event type");
            }
        }
    }
}
