using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagementSystem.Models
{
    public partial class Lease
    {
        public int LeaseId { get; set; }
        public int UserId { get; set; }
        public int BookId { get; set; }
        public DateTime LeaseStartDate { get; set; }
        public DateTime? LeaseEndDate { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; }


        public virtual Book Book { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }

}
