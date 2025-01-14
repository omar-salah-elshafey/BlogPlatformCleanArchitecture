namespace BlogPlatformCleanArchitecture.Application.ExceptionHandling
{
    public class DuplicateEmailException : Exception
    {
        public DuplicateEmailException(string message) : base(message) { }
    }
}
