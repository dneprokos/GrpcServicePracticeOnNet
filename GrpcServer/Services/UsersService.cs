using Grpc.Core;
using GrpcServer.DatabaseProviders;
using GrpcServer.Protos;
using GrpcServer.Validators;

namespace GrpcServer.Services
{
    public class UsersService : User.UserBase
    {
        private readonly Logger<UsersService>? _logger;
        private UsersDbProvider _usersDbProvider;

        public UsersService(Logger<UsersService>? logger = null)
        {
            _usersDbProvider = UsersDbProvider.GetInstance();
            _logger = logger;
        }

        /// <summary>
        /// Creates a user
        /// </summary>
        /// <param name="request">Request model</param>
        /// <param name="context">Server context</param>
        /// <returns></returns>
        /// <exception cref="RpcException"></exception>
        public override async Task<UserResponseModel> CreateUser(UserRequestModel request, ServerCallContext context)
        {
            // Validate properties
            var validator = new UserRequestValidator();
            var validationResult = validator.Validate(request);

            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage);
                string errorMessage = $"Validation errors: {string.Join(", ", errors)}";
                _logger?.LogWarning("CreateUser request received with validation errors: {ErrorMessage}", errorMessage);
                throw new RpcException(new Status(StatusCode.InvalidArgument, errorMessage));
            }

            // Proceed with creating the user if validation passes
            var response = _usersDbProvider.AddUserToDb(request);
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

            var dbResults = _usersDbProvider.GetAll();

            foreach (var user in dbResults)
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
            var dbResults = _usersDbProvider.GetAll();
            response.Users.AddRange(dbResults);

            return Task.FromResult(response);
        }
    }
}
