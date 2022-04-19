namespace NBasis.Lambda.RuntimeSupport
{
    public interface IFunctionRuntime : IDisposable
    {
        Task RunAsync(CancellationToken cancellationToken = default);
    }
}
