using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;

namespace NBasis.Lambda.RuntimeSupport
{
    public class MultiEventFunctionRuntime : BaseFunctionRuntime
    {
        public MultiEventFunctionRuntime(MultiEventFunction function, ILambdaSerializer serializer)
        {
            _boostrap = new LambdaBootstrap(async (invocation) =>
            {
                var multiEvent = new MultiEvent(invocation.InputStream);
                await function.FunctionHandler(multiEvent, invocation.LambdaContext);

                _outputStream.SetLength(0);
                _outputStream.Position = 0;
                return new InvocationResponse(_outputStream, false);
            });
        }
    }
}
