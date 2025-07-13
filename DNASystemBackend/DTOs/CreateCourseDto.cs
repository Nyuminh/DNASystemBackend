namespace DNASystemBackend.DTOs
{
    public class CreateCourseDto
    {
        

        public string? ManagerId { get; set; }

        public string? Title { get; set; }

        public DateTime? Date { get; set; }

        public string? Description { get; set; }

        public string? Image { get; set; }

        public IFormFile? picture { get; set; }

    }
}