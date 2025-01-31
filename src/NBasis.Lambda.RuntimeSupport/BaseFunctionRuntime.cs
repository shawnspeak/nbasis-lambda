using Amazon.Lambda.RuntimeSupport;

namespace NBasis.Lambda.RuntimeSupport
{
    public abstract class BaseFunctionRuntime : IFunctionRuntime
    {
        readonly protected MemoryStream _outputStream = new();

        protected LambdaBootstrap _boostrap;

        private bool _disposed;

        public Task RunAsync(CancellationToken cancellationToken = default)
        {
            return _boostrap.RunAsync(cancellationToken);
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
                    _boostrap.Dispose();
                    _outputStream.Dispose();
                }

                _disposed = true;
            }
        }
    }
}
