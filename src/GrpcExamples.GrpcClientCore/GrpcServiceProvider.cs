using Grpc.Net.Client;
using GrpcExamples.Server;

namespace GrpcExamples.GrpcClientCore
{
    public class GrpcServiceProvider : IDisposable
    {
        private Lazy<GrpcChannel> lazyGrpcChannel;
        private bool disposedValue;

        public GrpcServiceProvider(string serviceUrl)
        {
            lazyGrpcChannel = new Lazy<GrpcChannel>(GrpcChannel.ForAddress(serviceUrl));
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public Greeter.GreeterClient GetGreeterClient()
        {
            return new Greeter.GreeterClient(lazyGrpcChannel.Value);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (lazyGrpcChannel.IsValueCreated)
                    {
                        lazyGrpcChannel.Value.Dispose();
                    }
                }

                disposedValue = true;
            }
        }
    }
}