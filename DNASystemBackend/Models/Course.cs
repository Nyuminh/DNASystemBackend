using System;
using System.Collections.Generic;

namespace DNASystemBackend.Models;

public partial class Course
{
    public string CourseId { get; set; } = null!;

    public string? ManagerId { get; set; }

    public string? Title { get; set; }

    public DateTime? Date { get; set; }

    public string? Description { get; set; }

    public string? Image { get; set; }

    public virtual User? Manager { get; set; }
}
