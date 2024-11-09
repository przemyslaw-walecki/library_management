using System;
using System.Collections.Generic;

namespace LibraryManagementSystem.Models
{
    public partial class Reservation
    {
        public int ReservationId { get; set; }
        public int UserId { get; set; } 
        public int BookId { get; set; }
        public DateTime ReservationDate { get; set; }
        public DateTime ReservationEndDate { get; set; }

        public virtual Book Book { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
}
