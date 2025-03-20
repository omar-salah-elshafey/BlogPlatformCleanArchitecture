namespace BlogPlatformCleanArchitecture.Domain.Entities
{
    public class Otp
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string OtpCode { get; set; }
        public string Token { get; set; }
        public DateTime ExpirationDateTime { get; set; }
    }
}
