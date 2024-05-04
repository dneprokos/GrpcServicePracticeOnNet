using Bogus;
using FluentAssertions;
using FluentAssertions.Common;
using FluentAssertions.Execution;
using Grpc.Core;
using Grpc.Net.Client;
using GrpcServer.Protos;
using GrpcService.Tests.Utils.RandomGenerators;
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
            RpcException? exception = Assert.ThrowsAsync<RpcException>(async () =>
            {
                UserResponseModel? response = await _userClient!.CreateUserAsync(requestModel).ResponseAsync;
            });

            // Assert
            exception!.Status.StatusCode.Should().Be(StatusCode.InvalidArgument);
            exception.Status.Detail.Should().Be("Validation errors: Email is required, FirstName is required, First name min length is 3, LastName is required, Last name min length is 3, Age must be between 18 and 60");
        }

        [TestCase(3, true)]
        [TestCase(2, false)]
        [TestCase(100, true)]
        [TestCase(101, false)]
        public void CreateUser_LastName_ShouldWorkUpToValidation(int charsNumber, bool isPositive)
        {
            //Arrange
            var requestModel = new UserRequestModel
            {
                Email = "test@test.com",
                FirstName = "FirstName",
                LastName = RandomString.GenerateRandomString(charsNumber)
            };

            //Act and Assert
            if (isPositive)
            {
                // Expect no exception, and user creation should be successful.
                Assert.DoesNotThrowAsync(async () => await _userClient!.CreateUserAsync(requestModel).ResponseAsync);
            }
            else
            {
                // Expect an exception due to invalid age.
                var ex = Assert.ThrowsAsync<RpcException>(async () => await _userClient!.CreateUserAsync(requestModel).ResponseAsync);
                Assert.That(ex!.Status.StatusCode, Is.EqualTo(StatusCode.InvalidArgument));

                var expectedMessage = charsNumber == 101 ?
                    "Last name max length is 100" :
                    "Last name min length is 3";
                Assert.That(ex.Status.Detail, Does.Contain(expectedMessage));                  
            }
        }

        [TestCase(3, true)]
        [TestCase(2, false)]
        [TestCase(100, true)]
        [TestCase(101, false)]
        public void CreateUser_FirstName_ShouldWorkUpToValidation(int charsNumber, bool isPositive)
        {
            //Arrange
            var requestModel = new UserRequestModel
            {
                Email = "test@test.com",
                FirstName = RandomString.GenerateRandomString(charsNumber),
                LastName = "LastName"
            };

            //Act and Assert
            if (isPositive)
            {
                // Expect no exception, and user creation should be successful.
                Assert.DoesNotThrowAsync(async () => await _userClient!.CreateUserAsync(requestModel).ResponseAsync);
            }
            else
            {
                // Expect an exception due to invalid age.
                var ex = Assert.ThrowsAsync<RpcException>(async () => await _userClient!.CreateUserAsync(requestModel).ResponseAsync);
                Assert.That(ex!.Status.StatusCode, Is.EqualTo(StatusCode.InvalidArgument));

                var expectedMessage = charsNumber == 101 ?
                    "First name max length is 100" :
                    "First name min length is 3";
                Assert.That(ex.Status.Detail, Does.Contain(expectedMessage));
            }
        }

        [TestCase(18, true)]
        [TestCase(17, false)]
        [TestCase(60, true)]
        [TestCase(61, false)]
        public void CreateUser_AgeValidation_ShouldWorkUpToValidation(int age, bool isValid)
        {
            //Arrange
            var requestModel = new UserRequestModel
            {
                Email = "test@test.com",
                FirstName = "FirstName",
                LastName = "LastName",
                UserType = UserType.Regular,
                Age = age
            };

            // Act & Assert
            if (isValid)
            {
                // Expect no exception, and user creation should be successful.
                Assert.DoesNotThrowAsync(async () => await _userClient!.CreateUserAsync(requestModel).ResponseAsync);
            }
            else
            {
                // Expect an exception due to invalid age.
                var ex = Assert.ThrowsAsync<RpcException>(async () => await _userClient!.CreateUserAsync(requestModel).ResponseAsync);
                Assert.That(ex!.Status.StatusCode, Is.EqualTo(StatusCode.InvalidArgument));
                Assert.That(ex.Status.Detail, Does.Contain("Age must be between 18 and 60"));
            }
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
            RuleFor(u => u.Age, f => f.Person.Random.Int(18, 60));
            RuleFor(u => u.IsDiscount, f => f.Person.Random.Bool(0.5f));
            RuleFor(u => u.UserType, f => UserType.Regular);
        }
    }

}
