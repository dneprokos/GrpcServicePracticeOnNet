// See https://aka.ms/new-console-template for more information

using Grpc.Net.Client;
using GrpcServer;

var channel = GrpcChannel.ForAddress("http://localhost:5000");
var client = new Greeter.GreeterClient(channel);

var reply = client.SayHello(new HelloRequest { Name = "Kostia"} );
Console.WriteLine(reply.Message);


Console.ReadLine();
