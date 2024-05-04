using GrpcServer.Protos;
using System.Collections.Concurrent;

namespace GrpcServer.DatabaseProviders
{
    public class UsersDbProvider
    {
        private static UsersDbProvider? _instance;
        private static readonly object _lock = new();

        private ConcurrentBag<UserResponseModel> _tempDataBase = [];

        // Private constructor to prevent external instantiation
        private UsersDbProvider()
        {
            // Initialize your database or any other setup
        }

        // Method to get the singleton instance
        public static UsersDbProvider GetInstance()
        {
            // Double-check locking for thread safety
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new UsersDbProvider();
                    }
                }
            }
            return _instance;
        }

        public UserResponseModel AddUserToDb(UserRequestModel userData)
        {
            var response = new UserResponseModel
            {
                Email = userData.Email,
                FirstName = userData.FirstName,
                LastName = userData.LastName,
                Age = userData.Age,
                IsDiscount = userData.IsDiscount,
                Id = _tempDataBase.Count + 1,
                UserType = userData.UserType,
            };

            _tempDataBase.Add(response);

            return response;
        }

        public ConcurrentBag<UserResponseModel> GetAll() => _tempDataBase;
    }
}
