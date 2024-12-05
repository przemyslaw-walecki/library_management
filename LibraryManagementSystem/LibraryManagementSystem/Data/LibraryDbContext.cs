using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Data
{
    public partial class LibraryDbContext : DbContext
    {
        public LibraryDbContext()
        {
        }

        public LibraryDbContext(DbContextOptions<LibraryDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Book> Books { get; set; } = null!;
        public virtual DbSet<Lease> Leases { get; set; } = null!;
        public virtual DbSet<Reservation> Reservations { get; set; } = null!;
        public virtual DbSet<User> Users { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql("Name=ConnectionString");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Book>(entity =>
            {
                entity.ToTable("books");

                entity.Property(e => e.BookId).HasColumnName("book_id");

                entity.Property(e => e.Author)
                    .HasMaxLength(100)
                    .HasColumnName("author");

                entity.Property(e => e.DateOfPublication).HasColumnName("date_of_publication");

                entity.Property(e => e.IsPermanentlyUnavailable)
                    .HasColumnName("is_permanently_unavailable");

                entity.Property(e => e.Name)
                    .HasMaxLength(100)
                    .HasColumnName("name");

                entity.Property(e => e.Price)
                    .HasPrecision(10, 2)
                    .HasColumnName("price");

                entity.Property(e => e.Publisher)
                    .HasMaxLength(100)
                    .HasColumnName("publisher");

                entity.Property(e => e.RowVersion)
                    .IsRowVersion()
                    .IsConcurrencyToken()
                .HasColumnName("RowVersion").ValueGeneratedOnAddOrUpdate(); 
            });

            modelBuilder.Entity<Lease>(entity =>
            {
                entity.ToTable("leases");

                entity.Property(e => e.LeaseId).HasColumnName("lease_id");

                entity.Property(e => e.BookId).HasColumnName("book_id");

                entity.Property(e => e.LeaseEndDate)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("lease_end_date");

                entity.Property(e => e.LeaseStartDate)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("lease_start_date")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.UserId)
                    .HasMaxLength(250)
                    .HasColumnName("user_id");

                entity.HasOne(d => d.Book)
                    .WithMany(p => p.Leases)
                    .HasForeignKey(d => d.BookId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("leases_book_id_fkey");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Leases)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("leases_user_id_fkey");
            });

            modelBuilder.Entity<Reservation>(entity =>
            {
                entity.ToTable("reservations");

                entity.Property(e => e.ReservationId).HasColumnName("reservation_id");

                entity.Property(e => e.BookId).HasColumnName("book_id");

                entity.Property(e => e.ReservationDate)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("reservation_date")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.ReservationEndDate)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("reservation_expiry");

                entity.Property(e => e.UserId)
                    .HasMaxLength(250)
                    .HasColumnName("user_id");

                entity.HasOne(d => d.Book)
                    .WithMany(p => p.Reservations)
                    .HasForeignKey(d => d.BookId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("reservations_book_id_fkey");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Reservations)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("reservations_user_id_fkey");

            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");

                entity.HasIndex(e => e.Email, "users_email_key")
                    .IsUnique();

                entity.HasIndex(e => e.Username, "users_username_key")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasMaxLength(250)
                    .HasColumnName("id");

                entity.Property(e => e.Email)
                    .HasMaxLength(100)
                    .HasColumnName("email");

                entity.Property(e => e.FirstName)
                    .HasMaxLength(50)
                    .HasColumnName("first_name");

                entity.Property(e => e.IsLibrarian)
                    .HasColumnName("is_librarian");

                entity.Property(e => e.LastName)
                    .HasMaxLength(50)
                    .HasColumnName("last_name");

                entity.Property(e => e.Password)
                    .HasMaxLength(255)
                    .HasColumnName("password");

                entity.Property(e => e.PhoneNumber)
                    .HasMaxLength(20)
                    .HasColumnName("phone_number");

                entity.Property(e => e.Username)
                    .HasMaxLength(50)
                    .HasColumnName("username");

                entity.Property(e => e.Version)
                .HasColumnName("xmin").HasColumnType("xid").ValueGeneratedOnAddOrUpdate().IsConcurrencyToken();
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
