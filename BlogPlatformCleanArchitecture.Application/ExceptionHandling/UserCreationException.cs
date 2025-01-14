namespace BlogPlatformCleanArchitecture.Application.ExceptionHandling
{
    public class UserCreationException : Exception
    {
        public UserCreationException(string message) : base(message) { }
    }
}
