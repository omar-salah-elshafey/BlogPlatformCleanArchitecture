namespace BlogPlatformCleanArchitecture.Application.DTOs
{
    public record NotificationDto
    (
         int Id,
         string Message,
         string Type,
         int? RelatedPostId
    );
}
