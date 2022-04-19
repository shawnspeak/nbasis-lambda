using Amazon.Lambda.Core;

namespace NBasis.Lambda
{
    public interface IHandleLambdaRequest<TRequest, TResponse>
    {
        Task<TResponse> HandleRequestAsync(TRequest input, ILambdaContext context);
    }
}
