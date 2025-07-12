namespace DNASystemBackend.DTOs
{
    public class UpdateServiceDto
    {
        public string? Type { get; set; }

        public string? Name { get; set; }

        public decimal? Price { get; set; }

        public string? Description { get; set; }

        public string? Image { get; set; }

        public IFormFile? picture { get; set; }

    }
}