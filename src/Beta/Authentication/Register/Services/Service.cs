using Grpc.Core;
using Journal.Beta.Authentication.Register.Protos;

namespace Journal.Beta.Authentication.Register.Services;

public class Service:Greeter2.Greeter2Base
{
    private readonly ILogger<Service> _logger;
    public Service(ILogger<Service> logger)
    {
        _logger = logger;
    }

    public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
    {
        int a = 1, b = 2;
        int c = a + b;
        return Task.FromResult(new HelloReply
        {
            Message = "Hello " + request.Name + " " + c
        });
    }
}
