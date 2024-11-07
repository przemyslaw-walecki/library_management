using System;
using System.Collections.Generic;

namespace LibraryManagementSystem.Models
{
    public partial class Lease
    {
        public int LeaseId { get; set; }
        public string UserId { get; set; }
        public int BookId { get; set; }
        public DateTime? LeaseStartDate { get; set; }
        public DateTime? LeaseEndDate { get; set; }

        public virtual Book Book { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
}
