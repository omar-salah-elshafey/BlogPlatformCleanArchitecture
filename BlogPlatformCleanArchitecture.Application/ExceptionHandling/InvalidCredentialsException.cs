namespace BlogPlatformCleanArchitecture.Application.ExceptionHandling
{
    public class InvalidCredentialsException : Exception
    {
        public InvalidCredentialsException(string message) : base(message) { }
    }
}
