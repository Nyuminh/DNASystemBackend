using System;
using System.Collections.Generic;

namespace DNASystemBackend.Models;

public partial class Service
{
    public string ServiceId { get; set; } = null!;

    public string? Type { get; set; }

    public string? Name { get; set; }

    public decimal? Price { get; set; }

    public string? Description { get; set; }

    public string? Image { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public virtual ICollection<InvoiceDetail> InvoiceDetails { get; set; } = new List<InvoiceDetail>();

    public virtual ICollection<TestResult> TestResults { get; set; } = new List<TestResult>();
}
