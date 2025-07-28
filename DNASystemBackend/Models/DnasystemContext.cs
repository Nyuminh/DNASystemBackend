using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace DNASystemBackend.Models;

public partial class DnasystemContext : DbContext
{
    public DnasystemContext()
    {
    }

    public DnasystemContext(DbContextOptions<DnasystemContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Booking> Bookings { get; set; }

    public virtual DbSet<Course> Courses { get; set; }

    public virtual DbSet<Feedback> Feedbacks { get; set; }

    public virtual DbSet<Invoice> Invoices { get; set; }

    public virtual DbSet<InvoiceDetail> InvoiceDetails { get; set; }

    public virtual DbSet<Kit> Kits { get; set; }
    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Relative> Relatives { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Service> Services { get; set; }

    public virtual DbSet<TestResult> TestResults { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
            optionsBuilder.UseSqlServer(GetConnectionString());
        }
    }

    private string GetConnectionString()
    {
        return new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build()
            .GetConnectionString("DNASystemDb");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.BookingId).HasName("PK__Booking__C6D03BEDBC0383B6");

            entity.ToTable("Booking");

            entity.Property(e => e.BookingId)
                .HasMaxLength(10)
                .HasColumnName("bookingID");
            entity.Property(e => e.Address)
                .HasMaxLength(100)
                .HasColumnName("address");
            entity.Property(e => e.CustomerId)
                .HasMaxLength(10)
                .HasColumnName("customerID");
            entity.Property(e => e.Date)
                .HasColumnType("datetime")
                .HasColumnName("date");
            entity.Property(e => e.Method)
                .HasMaxLength(20)
                .HasColumnName("method");
            entity.Property(e => e.ServiceId)
                .HasMaxLength(10)
                .HasColumnName("serviceID");
            entity.Property(e => e.StaffId)
                .HasMaxLength(10)
                .HasColumnName("staffID");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasColumnName("status");

            entity.HasOne(d => d.Customer).WithMany(p => p.BookingCustomers)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("FK__Booking__custome__2739D489");

            entity.HasOne(d => d.Service).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.ServiceId)
                .HasConstraintName("FK__Booking__service__282DF8C2");

            entity.HasOne(d => d.Staff).WithMany(p => p.BookingStaffs)
                .HasForeignKey(d => d.StaffId)
                .HasConstraintName("FK__Booking__staffID__29221CFB");
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(e => e.CourseId).HasName("PK__Course__2AA84FF1FB1A1BF7");

            entity.ToTable("Course");

            entity.Property(e => e.CourseId)
                .HasMaxLength(10)
                .HasColumnName("courseID");
            entity.Property(e => e.Date)
                .HasColumnType("datetime")
                .HasColumnName("date");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Image)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("image");
            entity.Property(e => e.ManagerId)
                .HasMaxLength(10)
                .HasColumnName("managerID");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");

            entity.HasOne(d => d.Manager).WithMany(p => p.Courses)
                .HasForeignKey(d => d.ManagerId)
                .HasConstraintName("FK__Course__managerI__2A164134");
        });

        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.HasKey(e => e.FeedbackId).HasName("PK__Feedback__2613FDC4A6FF49A0");

            entity.ToTable("Feedback");

            entity.Property(e => e.FeedbackId)
                .HasMaxLength(10)
                .HasColumnName("feedbackID");
            entity.Property(e => e.Comment).HasColumnName("comment");
            entity.Property(e => e.CustomerId)
                .HasMaxLength(10)
                .HasColumnName("customerID");
            entity.Property(e => e.Rating).HasColumnName("rating");
            entity.Property(e => e.ServiceId)
                .HasMaxLength(10)
                .HasColumnName("serviceID");

            entity.HasOne(d => d.Customer).WithMany(p => p.Feedbacks)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("FK__Feedback__custom__4CA06362");

            entity.HasOne(d => d.Service).WithMany(p => p.Feedbacks)
                .HasForeignKey(d => d.ServiceId)
                .HasConstraintName("FK__Feedback__servic__2BFE89A6");
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.InvoiceId).HasName("PK__Invoice__1252410CEBF3D6F7");

            entity.ToTable("Invoice");

            entity.Property(e => e.InvoiceId)
                .HasMaxLength(10)
                .HasColumnName("invoiceID");
            entity.Property(e => e.BookingId)
                .HasMaxLength(10)
                .HasColumnName("bookingID");
            entity.Property(e => e.Date)
                .HasColumnType("datetime")
                .HasColumnName("date");
            entity.Property(e => e.Price)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("price");

            entity.HasOne(d => d.Booking).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.BookingId)
                .HasConstraintName("FK__Invoice__booking__2CF2ADDF");
        });

        modelBuilder.Entity<InvoiceDetail>(entity =>
        {
            entity.HasKey(e => e.InvoicedetailId).HasName("PK__InvoiceD__FDA5DC324B6CD873");

            entity.ToTable("InvoiceDetail");

            entity.Property(e => e.InvoicedetailId)
                .HasMaxLength(10)
                .HasColumnName("invoicedetailID");
            entity.Property(e => e.InvoiceId)
                .HasMaxLength(10)
                .HasColumnName("invoiceID");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.ServiceId)
                .HasMaxLength(10)
                .HasColumnName("serviceID");

            entity.HasOne(d => d.Invoice).WithMany(p => p.InvoiceDetails)
                .HasForeignKey(d => d.InvoiceId)
                .HasConstraintName("FK__InvoiceDe__invoi__4F7CD00D");

            entity.HasOne(d => d.Service).WithMany(p => p.InvoiceDetails)
                .HasForeignKey(d => d.ServiceId)
                .HasConstraintName("FK__InvoiceDe__servi__2EDAF651");
        });

        modelBuilder.Entity<Kit>(entity =>
        {
            entity.HasKey(e => e.KitId).HasName("PK__Kit__98C65C8031FC837A");

            entity.ToTable("Kit");

            entity.Property(e => e.KitId)
                .HasMaxLength(10)
                .HasColumnName("kitID");
            entity.Property(e => e.BookingId)
                .HasMaxLength(10)
                .HasColumnName("bookingID");
            entity.Property(e => e.CustomerId)
                .HasMaxLength(10)
                .HasColumnName("customerID");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Receivedate)
                .HasColumnType("datetime")
                .HasColumnName("receivedate");
            entity.Property(e => e.StaffId)
                .HasMaxLength(10)
                .HasColumnName("staffID");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");

            entity.HasOne(d => d.Booking).WithMany(p => p.Kits)
                .HasForeignKey(d => d.BookingId)
                .HasConstraintName("FK__Kit__bookingID__31B762FC");

            entity.HasOne(d => d.Customer).WithMany(p => p.KitCustomers)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("FK__Kit__customerID__2FCF1A8A");

            entity.HasOne(d => d.Staff).WithMany(p => p.KitStaffs)
                .HasForeignKey(d => d.StaffId)
                .HasConstraintName("FK__Kit__staffID__30C33EC3");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("PK__Notifica__B5B1A8A2D5B1A8A2");

            entity.ToTable("Notification");

            entity.Property(e => e.NotificationId)
                .HasMaxLength(10)
                .HasColumnName("notificationID");
            entity.Property(e => e.UserId)
                .HasMaxLength(10)
                .HasColumnName("userID");
            entity.Property(e => e.Title)
                .HasMaxLength(100)
                .HasColumnName("title");
            entity.Property(e => e.Message)
                .HasMaxLength(500)
                .HasColumnName("message");
            entity.Property(e => e.NotificationType)
                .HasMaxLength(50)
                .HasColumnName("notificationType");
            entity.Property(e => e.IsRead).HasColumnName("isRead");

            entity.HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
        });

        modelBuilder.Entity<Relative>(entity =>
        {
            entity.HasKey(e => e.RelativeId).HasName("PK__Relative__67BC160D7CAF50DA");

            entity.Property(e => e.RelativeId)
                .HasMaxLength(10)
                .HasColumnName("relativeID");
            entity.Property(e => e.Address)
                .HasMaxLength(100)
                .HasColumnName("address");
            entity.Property(e => e.Birthdate).HasColumnName("birthdate");
            entity.Property(e => e.BookingId)
                .HasMaxLength(10)
                .HasColumnName("bookingID");
            entity.Property(e => e.Fullname)
                .HasMaxLength(50)
                .HasColumnName("fullname");
            entity.Property(e => e.Gender)
                .HasMaxLength(10)
                .HasColumnName("gender");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("phone");
            entity.Property(e => e.Relationship)
                .HasMaxLength(50)
                .HasColumnName("relationship");
            entity.Property(e => e.UserId)
                .HasMaxLength(10)
                .HasColumnName("userID");

            entity.HasOne(d => d.Booking).WithMany(p => p.Relatives)
                .HasForeignKey(d => d.BookingId)
                .HasConstraintName("FK_Relatives_Booking");

            entity.HasOne(d => d.User).WithMany(p => p.Relatives)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Relatives_Users");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Role__CD98460A0267DDC4");

            entity.ToTable("Role");

            entity.Property(e => e.RoleId)
                .HasMaxLength(10)
                .HasColumnName("roleID");
            entity.Property(e => e.Rolename)
                .HasMaxLength(50)
                .HasColumnName("rolename");
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.HasKey(e => e.ServiceId).HasName("PK__Service__4550733F121AD4A3");

            entity.ToTable("Service");

            entity.Property(e => e.ServiceId)
                .HasMaxLength(10)
                .HasColumnName("serviceID");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Image)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("image");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.Price)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("price");
            entity.Property(e => e.Type)
                .HasMaxLength(50)
                .HasColumnName("type");
        });

        modelBuilder.Entity<TestResult>(entity =>
        {
            entity.HasKey(e => e.ResultId).HasName("PK__TestResu__C6EADC7BE2440D27");

            entity.ToTable("TestResult");

            entity.Property(e => e.ResultId)
                .HasMaxLength(10)
                .HasColumnName("resultID");
            entity.Property(e => e.BookingId)
                .HasMaxLength(10)
                .HasColumnName("bookingID");
            entity.Property(e => e.CustomerId)
                .HasMaxLength(10)
                .HasColumnName("customerID");
            entity.Property(e => e.Date)
                .HasColumnType("datetime")
                .HasColumnName("date");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.ServiceId)
                .HasMaxLength(10)
                .HasColumnName("serviceID");
            entity.Property(e => e.StaffId)
                .HasMaxLength(10)
                .HasColumnName("staffID");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");

            entity.HasOne(d => d.Booking).WithMany(p => p.TestResults)
                .HasForeignKey(d => d.BookingId)
                .HasConstraintName("FK__TestResul__booki__3587F3E0");

            entity.HasOne(d => d.Customer).WithMany(p => p.TestResultCustomers)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("FK__TestResul__custo__32AB8735");

            entity.HasOne(d => d.Service).WithMany(p => p.TestResults)
                .HasForeignKey(d => d.ServiceId)
                .HasConstraintName("FK__TestResul__servi__339FAB6E");

            entity.HasOne(d => d.Staff).WithMany(p => p.TestResultStaffs)
                .HasForeignKey(d => d.StaffId)
                .HasConstraintName("FK__TestResul__staff__3493CFA7");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__CB9A1CDFBB6ADB74");

            entity.Property(e => e.UserId)
                .HasMaxLength(10)
                .HasColumnName("userID");
            entity.Property(e => e.Address)
                .HasMaxLength(50)
                .HasColumnName("address");
            entity.Property(e => e.Birthdate).HasColumnName("birthdate");
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.Fullname)
                .HasMaxLength(50)
                .HasColumnName("fullname");
            entity.Property(e => e.Gender)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("gender");
            entity.Property(e => e.Image)
                .HasMaxLength(255)  
                .HasColumnName("image");
            entity.Property(e => e.Password)
                .HasMaxLength(20)
                .HasColumnName("password");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("phone");
            entity.Property(e => e.RoleId)
                .HasMaxLength(10)
                .HasColumnName("roleID");
            entity.Property(e => e.Username)
                .HasMaxLength(20)
                .HasColumnName("username");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK__Users__roleID__5812160E");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
