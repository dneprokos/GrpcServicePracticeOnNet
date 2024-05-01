using Grpc.Core;
using GrpcServer.Protos;
using System.Collections.Concurrent;

namespace GrpcServer.Services
{
    public class UsersService : User.UserBase
    {
        private readonly Logger<UsersService>? _logger;
        private ConcurrentBag<UserResponseModel> _tempDataBase = [];

        public UsersService()
        {
        }

        //public UsersService(Logger<UsersService> logger)
        //{
        //   _logger = logger;
        //}

        /// <summary>
        /// Creates a user
        /// </summary>
        /// <param name="request">Request model</param>
        /// <param name="context">Server context</param>
        /// <returns></returns>
        /// <exception cref="RpcException"></exception>
        public override async Task<UserResponseModel> CreateUser(UserRequestModel request, ServerCallContext context)
        {
            // List to keep track of missing fields
            List<string> missingFields = [];

            // Validate required fields
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                missingFields.Add("email");
            }
            if (string.IsNullOrWhiteSpace(request.FirstName))
            {
                missingFields.Add("first name");
            }
            if (string.IsNullOrWhiteSpace(request.LastName))
            {
                missingFields.Add("last name");
            }

            // Check if there are any missing fields and respond accordingly
            if (missingFields.Count > 0)
            {
                string errorMessage = $"The following required fields are missing: {string.Join(", ", missingFields)}";
                _logger?.LogWarning("CreateUser request received with missing fields: {ErrorMessage}", errorMessage);
                throw new RpcException(new Status(StatusCode.InvalidArgument, errorMessage));
            }

            // Proceed with creating the user if validation passes
            var response = new UserResponseModel
            {
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Age = request.Age,
                IsDiscount = request.IsDiscount,
                Id = _tempDataBase.Count + 1  // Simulating a user creation
            };

            _tempDataBase.Add(response);
            _logger?.LogInformation($"User created with ID {response.Id}");

            return await Task.FromResult(response);
        }

        /// <summary>
        /// Streams all users from the temporary database.
        /// </summary>
        /// <param name="request">Empty request model.</param>
        /// <param name="responseStream">Server-side stream to send response.</param>
        /// <param name="context">Server context for RPC.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public override async Task GetUsers(Empty request, IServerStreamWriter<UserResponseModel> responseStream, ServerCallContext context)
        {
            _logger?.LogInformation("Starting to stream users.");

            foreach (var user in _tempDataBase)
            {
                if (context.CancellationToken.IsCancellationRequested)
                {
                    _logger?.LogWarning("Stream canceled by client.");
                    break;
                }

                await responseStream.WriteAsync(user);
                // Optionally add a delay or some logging here if needed
            }

            _logger?.LogInformation("Finished streaming users.");
        }

        /// <summary>
        /// Returns all users as array
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<UserListResponse> GetUsersAsArray(Google.Protobuf.WellKnownTypes.Empty request, ServerCallContext context)
        {
            _logger?.LogInformation("Retrieving all users");

            var response = new UserListResponse();
            response.Users.AddRange(_tempDataBase);

            return Task.FromResult(response);
        }
    }
}
