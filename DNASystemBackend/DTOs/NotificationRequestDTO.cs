namespace DNASystemBackend.DTOs
{
    public class NotificationRequestDTO
    {
        public string UserId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string NotificationType { get; set; }
    }
}