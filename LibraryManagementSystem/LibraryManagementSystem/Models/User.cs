using Microsoft.EntityFrameworkCore.Metadata.Internal;
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
        public string Email { get; set; } = null!;

        [Column("is_librarian", TypeName = "boolean")]
        [Required]
        public bool IsLibrarian { get; set; } = false;

        public virtual ICollection<Lease> Leases { get; set; }
        public virtual ICollection<Reservation> Reservations { get; set; }
    }

    public class MyAccountDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public string Email { get; set; } = null!;
        public List<BookInfoDto> Reservations { get; set; } = new();
        public List<BookInfoDto> Leases { get; set; } = new();
    }
    public class UserRegisterDto
    {
        [Required]
        public string Username { get; set; } = null!;

        [Required]
        public string FirstName { get; set; } = null!;

        [Required]
        public string LastName { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;

        [Phone]
        public string? PhoneNumber { get; set; }

        [EmailAddress]
        [Required]
        public string Email { get; set; } = null!;

        [Required]
        public bool IsLibrarian { get; set; }
    }
    public class UserDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
}

}
