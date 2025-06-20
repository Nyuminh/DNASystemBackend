namespace DNASystemBackend.DTOs
{
    public class UpdateUserDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string RoleId { get; set; }
        public string? Email { get; set; }
        public string? Fullname { get; set; }
        public string? Phone { get; set; }
    }

}
