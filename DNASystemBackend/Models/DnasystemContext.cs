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

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Service> Services { get; set; }

    public virtual DbSet<TestResult> TestResults { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning
        => optionsBuilder.UseSqlServer("Server=localhost;Database=DNASystem;User Id=sa;Password=12345;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.BookingId).HasName("PK__Booking__C6D03BED3D71970F");

            entity.ToTable("Booking");

            entity.Property(e => e.BookingId)
                .HasMaxLength(10)
                .HasColumnName("bookingID");
            entity.Property(e => e.Address)
                .HasMaxLength(55)
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
                .HasConstraintName("FK__Booking__custome__48CFD27E");

            entity.HasOne(d => d.Service).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.ServiceId)
                .HasConstraintName("FK__Booking__service__49C3F6B7");

            entity.HasOne(d => d.Staff).WithMany(p => p.BookingStaffs)
                .HasForeignKey(d => d.StaffId)
                .HasConstraintName("FK__Booking__staffID__4AB81AF0");
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(e => e.CourseId).HasName("PK__Course__2AA84FF1D01564E0");

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
                .HasConstraintName("FK__Course__managerI__4BAC3F29");
        });

        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.HasKey(e => e.FeedbackId).HasName("PK__Feedback__2613FDC4934EC59B");

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
                .HasConstraintName("FK__Feedback__servic__4D94879B");
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.InvoiceId).HasName("PK__Invoice__1252410C3C2110F4");

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
                .HasConstraintName("FK__Invoice__booking__4E88ABD4");
        });

        modelBuilder.Entity<InvoiceDetail>(entity =>
        {
            entity.HasKey(e => e.InvoicedetailId).HasName("PK__InvoiceD__FDA5DC328F0320B0");

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
                .HasConstraintName("FK__InvoiceDe__servi__5070F446");
        });

        modelBuilder.Entity<Kit>(entity =>
        {
            entity.HasKey(e => e.KitId).HasName("PK__Kit__98C65C80F78FE4BC");

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
                .IsUnicode(false)
                .HasColumnName("status");

            entity.HasOne(d => d.Booking).WithMany(p => p.Kits)
                .HasForeignKey(d => d.BookingId)
                .HasConstraintName("FK__Kit__bookingID__534D60F1");

            entity.HasOne(d => d.Customer).WithMany(p => p.KitCustomers)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("FK__Kit__customerID__5165187F");

            entity.HasOne(d => d.Staff).WithMany(p => p.KitStaffs)
                .HasForeignKey(d => d.StaffId)
                .HasConstraintName("FK__Kit__staffID__52593CB8");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Role__CD98460A7739AFF5");

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
            entity.HasKey(e => e.ServiceId).HasName("PK__Service__4550733FC14C703E");

            entity.ToTable("Service");

            entity.Property(e => e.ServiceId)
                .HasMaxLength(10)
                .HasColumnName("serviceID");
            entity.Property(e => e.Description)
                .HasColumnType("text")
                .HasColumnName("description");
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
            entity.HasKey(e => e.ResultId).HasName("PK__TestResu__C6EADC7B6B179A7F");

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
                .IsUnicode(false)
                .HasColumnName("status");

            entity.HasOne(d => d.Booking).WithMany(p => p.TestResults)
                .HasForeignKey(d => d.BookingId)
                .HasConstraintName("FK__TestResul__booki__571DF1D5");

            entity.HasOne(d => d.Customer).WithMany(p => p.TestResultCustomers)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("FK__TestResul__custo__5441852A");

            entity.HasOne(d => d.Service).WithMany(p => p.TestResults)
                .HasForeignKey(d => d.ServiceId)
                .HasConstraintName("FK__TestResul__servi__5535A963");

            entity.HasOne(d => d.Staff).WithMany(p => p.TestResultStaffs)
                .HasForeignKey(d => d.StaffId)
                .HasConstraintName("FK__TestResul__staff__5629CD9C");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__CB9A1CDFAE263982");

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
                .IsUnicode(false)
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
