namespace GrpcServer.Constants
{
    public static class GlobalConstants
    {
        public const int NameMaxChars = 100;
        public const int NameMinChars = 3;

        public static string PropertyIsRequiredMessage(string property)
            => $"{property} is required";
    }
}
