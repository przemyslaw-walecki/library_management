using System;
using System.Collections.Generic;

namespace LibraryManagementSystem.Models
{
    public partial class Book
    {
        public Book()
        {
            Leases = new HashSet<Lease>();
            Reservations = new HashSet<Reservation>();
        }

        public int BookId { get; set; }
        public string Author { get; set; } = null!;
        public string? Publisher { get; set; }
        public DateOnly? DateOfPublication { get; set; }
        public decimal? Price { get; set; }
        public bool? IsPermanentlyUnavailable { get; set; }
        public string Name { get; set; } = null!;

        public virtual ICollection<Lease> Leases { get; set; }
        public virtual ICollection<Reservation> Reservations { get; set; }
    }
}
