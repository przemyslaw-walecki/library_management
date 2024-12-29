using Microsoft.AspNetCore.Mvc;
using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using LibraryManagementSystem.Data;
using Microsoft.EntityFrameworkCore;


namespace LibraryManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly LibraryDbContext _context;

        public BooksController(LibraryDbContext context)
        {
            _context = context;
        }

        // GET: api/books
        [HttpGet]
        public async Task<IActionResult> GetBooks([FromQuery] string? searchString)
        {
            var books = _context.Books.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                books = books.Where(b => b.Name.Contains(searchString) ||
                                         b.Author.Contains(searchString) ||
                                         (b.Publisher != null && b.Publisher.Contains(searchString)) && b.IsPermanentlyUnavailable == false);
            }

            var bookList = await books.ToListAsync();

            foreach (var book in bookList)
            {
                var activeReservation = await _context.Reservations
                    .FirstOrDefaultAsync(r => r.BookId == book.BookId && r.ReservationEndDate > DateTime.Now);

                var activeLease = await _context.Leases
                    .FirstOrDefaultAsync(l => l.BookId == book.BookId && l.LeaseEndDate == null);

                book.IsReserved = activeReservation != null;
                book.IsLeased = activeLease != null;
            }

            return Ok(bookList);
        }

        [HttpPost("reserve")]
        public async Task<IActionResult> ReserveBook([FromBody] int bookId)
        {
            var userId = HttpContext.User?.Claims?.FirstOrDefault(c => c.Type == "userId")?.Value;

            if (userId == null)
            {
                return Unauthorized("You must be logged in to reserve a book.");
            }

            var book = await _context.Books.FirstOrDefaultAsync(b => b.BookId == bookId);

            if (book == null || book.IsPermanentlyUnavailable)
            {
                return NotFound("Book not found.");
            }

            var existingReservation = await _context.Reservations
                .FirstOrDefaultAsync(r => r.BookId == bookId);
            var existingLease = await _context.Leases.FirstOrDefaultAsync(l => l.BookId == bookId && l.LeaseEndDate < DateTime.Now);

            if (existingReservation != null || existingLease != null)
            {
                return Conflict("This book is already leased or reserved.");
            }

            var newReservation = new Reservation
            {
                BookId = bookId,
                UserId = int.Parse(userId),  // Assuming userId is stored as a claim
                ReservationDate = DateTime.Now,
                ReservationEndDate = DateTime.Now.Date.AddDays(2).AddSeconds(-1)
            };

            _context.Reservations.Add(newReservation);
            await _context.SaveChangesAsync();

            return Ok("Book reserved successfully!");
        }

        // GET: api/books/manage
        [HttpGet("manage")]
        [Authorize(Roles = "Librarian")]
        public async Task<IActionResult> GetAllBooksForManagement()
        {
            var books = await _context.Books.ToListAsync();
            return Ok(books);
        }

        // POST: api/books/add
        [HttpPost("add")]
        [Authorize(Roles = "Librarian")]
        public async Task<IActionResult> AddBook([FromBody] Book book)
        {
            if (book == null)
            {
                return BadRequest("Invalid book data.");
            }

            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBooks), new { id = book.BookId }, book);
        }

        // PUT: api/books/edit/{id}
        [HttpPut("edit/{id}")]
        [Authorize(Roles = "Librarian")]
        public async Task<IActionResult> EditBook(int id, [FromBody] Book book)
        {
            if (id != book.BookId)
            {
                return BadRequest("Book ID mismatch.");
            }

            var existingBook = await _context.Books.FindAsync(id);

            if (existingBook == null)
            {
                return NotFound("Book not found.");
            }

            existingBook.Author = book.Author;
            existingBook.Publisher = book.Publisher;
            existingBook.DateOfPublication = book.DateOfPublication;
            existingBook.Price = book.Price;
            existingBook.IsPermanentlyUnavailable = book.IsPermanentlyUnavailable;
            existingBook.Name = book.Name;

            await _context.SaveChangesAsync();

            return Ok("Book updated successfully.");
        }

        // DELETE: api/books/delete/{id}
        [HttpDelete("delete/{id}")]
        [Authorize(Roles = "Librarian")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var book = await _context.Books
                .Include(b => b.Leases)
                .FirstOrDefaultAsync(b => b.BookId == id);

            if (book == null || book.IsPermanentlyUnavailable)
            {
                return NotFound("Book not found or already marked as permanently unavailable.");
            }

            var activeLease = book.Leases.Any(l => l.LeaseEndDate == null);
            if (activeLease)
            {
                return Conflict("Book currently leased.");
            }

            if (book.Leases.Any())
            {
                book.IsPermanentlyUnavailable = true;
            }
            else
            {
                _context.Books.Remove(book);
            }

            await _context.SaveChangesAsync();
            return Ok("Book deleted/marked as unavailable successfully.");
        }
    }
}
