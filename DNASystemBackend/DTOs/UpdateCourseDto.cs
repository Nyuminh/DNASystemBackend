namespace DNASystemBackend.DTOs
{
    public class UpdateCourseDto
    {
        public string? ManagerId { get; set; }

        public string? Title { get; set; }

        public DateTime? Date { get; set; }

        public string? Description { get; set; }

        public string? Image { get; set; }
    
    }
}