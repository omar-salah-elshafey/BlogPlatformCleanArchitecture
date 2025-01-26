namespace BlogPlatformCleanArchitecture.Application.ExceptionHandling
{
    public class InvalidInputsException: Exception
    {
        public InvalidInputsException(string message) : base(message) { }
    }
}
