using System;
using System.Collections.Generic;

namespace LibraryManagementSystem.Models
{
    public partial class User
    {
        public User()
        {
            Leases = new HashSet<Lease>();
            Reservations = new HashSet<Reservation>();
        }

        public int UserId { get; set; }
        public string Username { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public string Password { get; set; } = null!;

        public virtual ICollection<Lease> Leases { get; set; }
        public virtual ICollection<Reservation> Reservations { get; set; }
    }
}
