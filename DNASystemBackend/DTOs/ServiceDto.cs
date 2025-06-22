namespace DNASystemBackend.DTOs
{
    public class ServiceDto
    {
        public string ServiceId { get; set; } = null!;

        public string? Type { get; set; }

        public string? Name { get; set; }

        public decimal? Price { get; set; }

        public string? Description { get; set; }

        public string? Image { get; set; }
    }
}