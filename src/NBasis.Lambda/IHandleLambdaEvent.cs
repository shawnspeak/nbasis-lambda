using Amazon.Lambda.Core;

namespace NBasis.Lambda
{
    public interface IHandleLambdaEvent<TEvent>
    {
        Task HandleEventAsync(TEvent input, ILambdaContext context);
    }
}
