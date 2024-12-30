using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

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

        [JsonIgnore]
        public virtual ICollection<Lease> Leases { get; set; }
        [JsonIgnore]
        public virtual ICollection<Reservation> Reservations { get; set; }
        
        public bool IsLeased => Leases.Any(l => l.LeaseEndDate == null);
        public bool IsReserved => Reservations.Any(r => r.ReservationEndDate > DateTime.Now);

    }
    public class BookListDTO
    {
        public int BookId { get; set; }
        public string Author { get; set; } = null!;
        public string? Publisher { get; set; }
        public DateTime? DateOfPublication { get; set; }
        public decimal Price { get; set; }
        public bool IsPermanentlyUnavailable { get; set; }
        public string Name { get; set; } = null!;
        public bool IsLeased { get; set; }
        public bool IsReserved { get; set; }
    }

    public class BookEditDto
    {
        public int BookId { get; set; }
        public string Author { get; set; } = null !;
        public string? Publisher { get; set; }
        public String? DateOfPublication { get; set; }
        public decimal Price { get; set; }
        public string Name { get; set; } = null!;
    }

}
