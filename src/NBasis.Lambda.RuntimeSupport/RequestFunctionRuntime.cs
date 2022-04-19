using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;

namespace NBasis.Lambda.RuntimeSupport
{
    public class RequestFunctionRuntime<TRequest, TResponse> : BaseFunctionRuntime
    {
        public RequestFunctionRuntime(ILambdaRequestFunction<TRequest, TResponse> function, ILambdaSerializer serializer)
        {
            _boostrap = new LambdaBootstrap(async (invocation) =>
            {
                TRequest input = serializer.Deserialize<TRequest>(invocation.InputStream);
                TResponse output = await function.FunctionHandler(input, invocation.LambdaContext);

                _outputStream.SetLength(0);
                serializer.Serialize(output, _outputStream);
                _outputStream.Position = 0;
                return new InvocationResponse(_outputStream, false);
            });
        }
    }
}
