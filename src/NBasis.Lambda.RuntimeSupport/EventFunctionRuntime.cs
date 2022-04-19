using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;

namespace NBasis.Lambda.RuntimeSupport
{
    public class EventFunctionRuntime<TEvent> : BaseFunctionRuntime
    {
        public EventFunctionRuntime(ILambdaEventFunction<TEvent> function, ILambdaSerializer serializer)
        {
            _boostrap = new LambdaBootstrap(async (invocation) =>
            {
                TEvent input = serializer.Deserialize<TEvent>(invocation.InputStream);
                await function.FunctionHandler(input, invocation.LambdaContext);

                _outputStream.SetLength(0);
                _outputStream.Position = 0;
                return new InvocationResponse(_outputStream, false);
            });
        }
    }
}
