namespace DNASystemBackend.DTOs
{
    public class CreateFeedbackDto
    {
        public string? CustomerId { get; set; }
        public string? ServiceId { get; set; }
        public string? Comment { get; set; }
        public int? Rating { get; set; }
    }
}
