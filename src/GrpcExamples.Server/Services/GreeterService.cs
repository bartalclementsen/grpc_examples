using Grpc.Core;
using GrpcExamples.Server;

namespace GrpcExamples.Server.Services
{
    public class GreeterService : Greeter.GreeterBase
    {
        private readonly ILogger<GreeterService> _logger;
        public GreeterService(ILogger<GreeterService> logger)
        {
            _logger = logger;
        }

        public override async Task SayHelloStream2(SayHelloStream2Request request, IServerStreamWriter<HelloReply> responseStream, ServerCallContext context)
        {
            try
            {
                connections.Add(responseStream);

                TaskCompletionSource tcs = new TaskCompletionSource();
                context.CancellationToken.Register(() =>
                {
                    tcs.TrySetCanceled();
                });
                await tcs.Task;
            }
            finally
            {
                connections.Remove(responseStream);
            }
        }

        public override async Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            var reply = BuildReply(request);
            await SendReplyToAllAsync(BuildReply(request));
            return reply;
        }

        private static readonly List<IServerStreamWriter<HelloReply>> connections = new List<IServerStreamWriter<HelloReply>>();

        public override async Task SayHelloStream(IAsyncStreamReader<HelloRequest> requestStream, IServerStreamWriter<HelloReply> responseStream, ServerCallContext context)
        {
            try
            {
                connections.Add(responseStream);

                while (await requestStream.MoveNext())
                {
                    var request = requestStream.Current;
                    await SendReplyToAllAsync(BuildReply(request));
                }
            }
            finally
            {
                connections.Remove(responseStream);
            }
        }

        private HelloReply BuildReply(HelloRequest request)
        {
            HelloReply reply;

            if (request.Age < 18)
            {
                reply = new HelloReply
                {
                    Message = "POGGERS " + request.Name
                };
            }
            else
            {
                reply = new HelloReply
                {
                    Message = "Hello " + request.Name
                };
            }


            switch (request.RequestCase)
            {
                case HelloRequest.RequestOneofCase.Standard:
                    reply.Standard = new HelloReplyStandard
                    {
                        Message = "Hello " + request.Name
                    };
                    break;
                case HelloRequest.RequestOneofCase.Advanced:
                    reply.Advanced = new HelloReplyAdvanced
                    {
                        Message = $"[{request.Advanced.From}] (to {request.Advanced.To}) {request.Advanced.Message}"
                    };
                    break;
            }

            return reply;
        }
        
        private static async Task SendReplyToAllAsync(HelloReply reply)
        {
            foreach (var connection in connections)
            {
                await connection.WriteAsync(reply);
            }
        }
    }
}