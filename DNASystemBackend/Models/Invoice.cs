using System;
using System.Collections.Generic;

namespace DNASystemBackend.Models;

public partial class Invoice
{
    public string InvoiceId { get; set; } = null!;

    public string? BookingId { get; set; }

    public DateTime? Date { get; set; }

    public decimal? Price { get; set; }

    public virtual Booking? Booking { get; set; }

    public virtual ICollection<InvoiceDetail> InvoiceDetails { get; set; } = new List<InvoiceDetail>();
}
