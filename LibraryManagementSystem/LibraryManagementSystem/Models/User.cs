using Microsoft.EntityFrameworkCore.Metadata.Internal;
using NuGet.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagementSystem.Models
{
    public partial class User
    {
        public User()
        {
            Leases = new HashSet<Lease>();
            Reservations = new HashSet<Reservation>();
        }

        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("username")]

        public string Username { get; set; } = null!;

        [Required]
        [Column("first_name")]
        public string FirstName { get; set; } = null!;

        [Required]
        [Column("last_name")]
        public string LastName { get; set; } = null!;

        [Required]
        [Column("password")]
        public string Password { get; set; } = null!;

        [Phone]
        [Column("phone_number")]
        public string? PhoneNumber { get; set; } = null!;

        [EmailAddress]
        [Column("email")]
        public string? Email { get; set; }

        [Column("is_librarian", TypeName = "boolean")]
        [Required]
        public bool IsLibrarian { get; set; } = false;

        public virtual ICollection<Lease> Leases { get; set; }
        public virtual ICollection<Reservation> Reservations { get; set; }

        [Timestamp]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [Column("xmin", TypeName = "xid")]
        public uint Version { get; set; }
    }
}
