using System;
using System.Collections.Generic;

namespace DNASystemBackend.Models;

public partial class User
{
    public string UserId { get; set; } = null!;

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string? Fullname { get; set; }

    public string? Gender { get; set; }

    public string? RoleId { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public DateOnly? Birthdate { get; set; }

    public string? Image { get; set; }

    public string? Address { get; set; }

    public virtual ICollection<Booking> BookingCustomers { get; set; } = new List<Booking>();

    public virtual ICollection<Booking> BookingStaffs { get; set; } = new List<Booking>();

    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();

    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public virtual ICollection<Kit> KitCustomers { get; set; } = new List<Kit>();

    public virtual ICollection<Kit> KitStaffs { get; set; } = new List<Kit>();

    public virtual ICollection<Relative> Relatives { get; set; } = new List<Relative>();

    public virtual Role? Role { get; set; }

    public virtual ICollection<TestResult> TestResultCustomers { get; set; } = new List<TestResult>();

    public virtual ICollection<TestResult> TestResultStaffs { get; set; } = new List<TestResult>();
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
