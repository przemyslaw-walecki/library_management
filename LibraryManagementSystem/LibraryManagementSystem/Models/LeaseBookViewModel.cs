namespace LibraryManagementSystem.Models
{
    public class LeaseBookViewModel
    {
        public int BookId { get; set; }
        public string Username { get; set; }
        public string? ReservedUsername { get; set; }  
    }
}