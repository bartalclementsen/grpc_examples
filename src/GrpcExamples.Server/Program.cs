using GrpcExamples.Server.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(o => o.AddPolicy("AllowAll", builder =>
{
    builder
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader()
        .WithExposedHeaders("Grpc-Status", "Grpc-Message", "Grpc-Encoding", "Grpc-Accept-Encoding");
}));

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();

var app = builder.Build();
IWebHostEnvironment env = app.Environment;

app.UseCors();

app.UseGrpcWeb();

if (env.IsDevelopment())
{
    app.MapGrpcReflectionService();
}

app.MapGrpcService<GreeterService>()
    .EnableGrpcWeb()
    .RequireCors("AllowAll");

app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();