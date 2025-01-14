namespace BlogPlatformCleanArchitecture.Application.ExceptionHandling
{
    public class UnauthorizedAccessException : Exception
    {
        public UnauthorizedAccessException(string message) : base(message) { }
    }
}
