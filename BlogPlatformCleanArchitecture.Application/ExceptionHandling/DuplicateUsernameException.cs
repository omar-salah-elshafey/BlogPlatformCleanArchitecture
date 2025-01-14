namespace BlogPlatformCleanArchitecture.Application.ExceptionHandling
{
    public class DuplicateUsernameException : Exception
    {
        public DuplicateUsernameException(string message) : base(message) { }
    }
}
