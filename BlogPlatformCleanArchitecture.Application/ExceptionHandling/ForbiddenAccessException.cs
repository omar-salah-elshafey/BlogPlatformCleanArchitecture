namespace BlogPlatformCleanArchitecture.Application.ExceptionHandling
{
    public class ForbiddenAccessException : Exception
    {
        public ForbiddenAccessException(string message) : base(message) { }
    }
}
