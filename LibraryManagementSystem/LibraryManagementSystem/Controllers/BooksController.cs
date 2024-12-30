using Microsoft.AspNetCore.Mvc;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Newtonsoft.Json;

namespace LibraryManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly LibraryDbContext _context;
        private readonly IConfiguration _configuration;

        public BooksController(LibraryDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        private int? GetUserIdFromJwt()
        {
            if (Request.Cookies.TryGetValue("AuthToken", out var token))
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);

                try
                {
                    var claimsPrincipal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ClockSkew = TimeSpan.Zero
                    }, out _);

                    var userIdClaim = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                    return int.TryParse(userIdClaim, out var userId) ? userId : null;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    return null;
                }
            }
            return null;
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

            var bookList = await books
                        .Include(b => b.Leases)
                        .Include(b => b.Reservations)
                        .ToListAsync();

            var bookDtos = books.Select(b => new BookListDTO
            {
                BookId = b.BookId,
                Author = b.Author,
                Publisher = b.Publisher,
                DateOfPublication = b.DateOfPublication,
                Price = b.Price,
                IsPermanentlyUnavailable = b.IsPermanentlyUnavailable,
                Name = b.Name,
                IsLeased = b.IsLeased,
                IsReserved = b.IsReserved
            }).ToList();

            return Ok(bookDtos);
        }

        // GET: api/books/{id}
        [HttpGet("{id}")]
        [Authorize(Roles = "Librarian")]
        public async Task<IActionResult> GetBookById(int id)
        {
            var book = await _context.Books
                .Include(b => b.Leases)
                .Include(b => b.Reservations)
                .FirstOrDefaultAsync(b => b.BookId == id);

            if (book == null)
            {
                return NotFound("Book not found.");
            }


            var bookDto = new BookEditDto
            {
                BookId = book.BookId,
                Author = book.Author,
                Publisher = book.Publisher,
                DateOfPublication = book.DateOfPublication?.ToString("yyyy-MM-dd"),
                Price = book.Price,
                Name = book.Name
            };

            return Ok(bookDto);
        }


        // POST: api/books/reserve
        [HttpPost("reserve")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> ReserveBook([FromBody] int bookId)
        {
            var userId = GetUserIdFromJwt();
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
                UserId = userId.Value,
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
            var books = await _context.Books
                .Include(b => b.Leases)
                .Include(b => b.Reservations)
                .ToListAsync();

            var bookDtos = books.Select(b => new BookListDTO
            {
                BookId = b.BookId,
                Author = b.Author,
                Publisher = b.Publisher,
                DateOfPublication = b.DateOfPublication,
                Price = b.Price,
                IsPermanentlyUnavailable = b.IsPermanentlyUnavailable,
                Name = b.Name,
                IsLeased = b.IsLeased,
                IsReserved = b.IsReserved
            }).ToList();

            return Ok(bookDtos);
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
        public async Task<IActionResult> EditBook(int id, [FromBody] BookEditDto book)
        {
            Console.WriteLine("Received Book DTO: " + JsonConvert.SerializeObject(book));
            var existingBook = await _context.Books.FindAsync(id);

            if (existingBook == null)
            {
                return NotFound("Book not found.");
            }
            var parsedDate = existingBook.DateOfPublication;
            if (!string.IsNullOrEmpty(book.DateOfPublication))
            {
                parsedDate = DateTime.Parse(book.DateOfPublication);
            }

            existingBook.Author = book.Author;
            existingBook.Publisher = book.Publisher;
            existingBook.DateOfPublication = parsedDate;
            existingBook.Price = book.Price;
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
