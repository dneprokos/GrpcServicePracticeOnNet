using Bogus;
using FluentAssertions;
using FluentAssertions.Execution;
using Grpc.Net.Client;
using GrpcServer.Protos;
using NUnit.Framework;

namespace GrpcService.Tests.Tests.UsersServiceTests
{
    [TestFixture]
    public class CreateUsersTests
    {
        private GrpcChannel? _channel;
        private User.UserClient? _userClient;

        [SetUp]
        public void SetUp()
        {
            // Setup new channel and client for each test
            _channel = GrpcChannel.ForAddress(TestContext.Parameters["server"]!);
            _userClient = new User.UserClient(_channel);
        }

        [TearDown]
        public void TearDown()
        {
            // Dispose the channel after each test
            _channel?.Dispose();
        }


        [Test]
        public async Task CreateUser_WithFullValidData_ShouldBeCreated()
        {
            //Arrange
            UserRequestModel requestModel = new UserFaker().Generate();


            //Act
            UserResponseModel? response = await _userClient!.CreateUserAsync(requestModel).ResponseAsync;

            //Assert
            response.Should().NotBeNull();
            using (new AssertionScope())
            {
                response.Email.Should().Be(requestModel.Email);
                response.FirstName.Should().Be(requestModel.FirstName);
                response.LastName.Should().Be(requestModel.LastName);
                response.Age.Should().Be(requestModel.Age);
                response.IsDiscount.Should().Be(requestModel.IsDiscount);
                response.Id.Should().BeGreaterThan(0);
            }
        }

        [Test]
        public async Task CreateUser_WithRequiredOnlyValidData_ShouldBeCreated()
        {
            //Arrange
            var requestModel = new UserRequestModel
            {
                Email = "test@test.com",
                FirstName = "FirstName",
                LastName = "LastName"
            };


            //Act
            UserResponseModel? response = await _userClient!.CreateUserAsync(requestModel).ResponseAsync;

            //Assert
            response.Should().NotBeNull();
            using (new AssertionScope())
            {
                response.Email.Should().Be(requestModel.Email);
                response.FirstName.Should().Be(requestModel.FirstName);
                response.LastName.Should().Be(requestModel.LastName);
                response.Age.Should().BeNull();
                response.IsDiscount.Should().BeFalse();
                response.Id.Should().BeGreaterThan(0);
            }
        }

        [Test]
        public void CreateUser_WithMissingRequiredFields_ShouldNotBeCreated()
        {
            //Arrange
            var requestModel = new UserRequestModel
            {
                Age = 1
            };

            // Act & Assert
            Grpc.Core.RpcException? exception = Assert.ThrowsAsync<Grpc.Core.RpcException>(async () =>
            {
                UserResponseModel? response = await _userClient!.CreateUserAsync(requestModel).ResponseAsync;
            });

            // Assert
            exception!.Status.StatusCode.Should().Be(Grpc.Core.StatusCode.InvalidArgument);
            exception.Status.Detail.Should().Be("The following required fields are missing: email, first name, last name");
        }

        //TODO: Add more tests...
    }

    public class UserFaker : Faker<UserRequestModel> 
    {
        public UserFaker()
        {
            RuleFor(u => u.Email, f => f.Person.Email);
            RuleFor(u => u.FirstName, f => f.Person.FirstName);
            RuleFor(u => u.LastName, f => f.Person.LastName);
            RuleFor(u => u.Age, f => f.Person.Random.Int(1, 99));
            RuleFor(u => u.IsDiscount, f => f.Person.Random.Bool(0.5f));
        }
    }

}
