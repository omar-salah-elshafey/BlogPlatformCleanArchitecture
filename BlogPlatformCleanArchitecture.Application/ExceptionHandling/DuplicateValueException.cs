namespace BlogPlatformCleanArchitecture.Application.ExceptionHandling
{
    public class DuplicateValueException : Exception
    {
        public DuplicateValueException(string message) : base(message) { }
    }
}
