namespace BlogPlatformCleanArchitecture.Application.ExceptionHandling
{
    public class UserNotFoundException : Exception
    {
        public UserNotFoundException(string message) : base(message) { }
    }
}
