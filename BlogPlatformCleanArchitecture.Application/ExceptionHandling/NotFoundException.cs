namespace BlogPlatformCleanArchitecture.Application.ExceptionHandling
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) { }
    }
}
