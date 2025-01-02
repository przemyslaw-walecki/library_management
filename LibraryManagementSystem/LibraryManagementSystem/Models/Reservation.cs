using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagementSystem.Models
{
    public partial class Reservation
    {
        [Column ("reservation_id")]
        public int ReservationId { get; set; }
        public int UserId { get; set; } 
        public int BookId { get; set; }
        public DateTime ReservationDate { get; set; }
        public DateTime ReservationEndDate { get; set; }

        public virtual Book Book { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
    public class ReservationDto
    {
        public int ReservationId { get; set; }
        public DateTime ReservationEndDate { get; set; }
        public BookInfoDto Book { get; set; } = null!;
        public UserDto User { get; set; } = null!;
    }

    public class LeaseDto
    {
        public int LeaseId { get; set; }
        public DateTime LeaseStartDate { get; set; }
        public DateTime? LeaseEndDate { get; set; }
        public BookInfoDto Book { get; set; } = null!;
        public UserDto User { get; set; } = null!;
    }
}
