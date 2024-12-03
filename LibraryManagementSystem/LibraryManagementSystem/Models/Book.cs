﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        [DataType(DataType.Date)]
        [Column("date_of_publication")]
        public DateTime? DateOfPublication { get; set; }
        public decimal Price { get; set; }
        public bool IsPermanentlyUnavailable { get; set; }
        public string Name { get; set; } = null!;
        
        public virtual ICollection<Lease> Leases { get; set; }
        public virtual ICollection<Reservation> Reservations { get; set; }

        public bool IsLeased;
        public bool IsReserved;

        [Timestamp]
        [Column("RowVersion")]
        public byte[] RowVersion { get; set; }
    }
}
