# GrpcServicePracticeOnNet

![MAIN](/images/main.png)

## Summary
Sample project with GRPC services server side and test clients. Both were implemented on NET8

- Both clients and server use copy of the same ".proto" file/contracts. 
- These files define an interface
- In real world, In real world, .proto file can be a part of the shared NuGet package. Out solutio  contain both Server and Client for simplification

## Projects

### GrpcClient
Just a sample console application that was added to test default Greeter service. 

### GrpcServer
GRPC server implementation. YOu should run it first.

#### How to run

##### From Visual Studio
1) In Visual Studio --> Configure Startup project--> Make sure server project is selected

![Run in Visual Studio](/images/grpcclient_visualstudio.png)

2) Click "Start" arrow button

##### From command line

1) Open CMD for the Server project
2) Run build command "dotnet build"
3) Run start command "dotnet run"

- Using Powershell script 
1) Open Powerhshell for Server project
2) Run "Start.ps1" powershell script

### GrpcService.Tests
NUnit test project was build as POC. The idea was to use GRPC proto contract and implement client and tests

#### Precondtions

1) Make sure latest version of ".proto" contract file was specified in protos folder.
 
 ![Client proto location](/images/client_proto_location.png)

2) Make sure Client proto configuration is equal to: Build Action = Protobug compiler, gRPC Stub Classes = Client only
 
 ![Client proto configuration](/images/client_proto_configuration.png)

3) Make sure server is running. See "How to run" section for server project


#### How to run

##### Usign Visual Studio Test Explorer

1) Select .runsettings file you want to run with, on top navigation menu -> "Test" -> "Configure Run Settings" --> Select file from the project root
2) Open Test Explorer window, on top navigation menu -> "Test" -> "Test Explorer"
3) Choice test you want to run or all tests scope
4) Right button context menu and click run

![Test Explorer](/images/test_explorer.png)

##### Using Command line

1) Open CMD in the root of the test project
2) Type "dotnet test -s "./localhost_5000.runsettings"", where -s is a location of .runsettings test configuration file

![Run from CMD](/images/from_cmd.png)


