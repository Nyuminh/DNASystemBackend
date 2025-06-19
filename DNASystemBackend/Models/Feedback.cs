using System;
using System.Collections.Generic;

namespace DNASystemBackend.Models;

public partial class Feedback
{
    public string FeedbackId { get; set; } = null!;

    public string? CustomerId { get; set; }

    public string? ServiceId { get; set; }

    public string? Comment { get; set; }

    public int? Rating { get; set; }

    public virtual User? Customer { get; set; }

    public virtual Service? Service { get; set; }
}
