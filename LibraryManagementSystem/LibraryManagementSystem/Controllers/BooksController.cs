using Microsoft.AspNetCore.Mvc;
using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Http;
using LibraryManagementSystem.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;

namespace YourProject.Controllers
{
    public class BooksController : Controller
    {
        private readonly LibraryDbContext _context;

        public BooksController(LibraryDbContext context)
        {
            _context = context;
        }

        private bool IsUserLibrarianAsync()
        {
            var userId = HttpContext.Session.GetInt32(SessionData.SessionKeyUserId);
            var isLibrarian = HttpContext.Session.GetInt32(SessionData.SessionKeyIsLibrarian);
            if (!userId.HasValue || !isLibrarian.HasValue || isLibrarian == 0)
            {
                return false;
            }
            return true;
        }

        public async Task<IActionResult> List(string searchString)
        {

            var books = _context.Books.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                books = books.Where(b => b.Name.Contains(searchString) ||
                                         b.Author.Contains(searchString) ||
                                         (b.Publisher != null && b.Publisher.Contains(searchString)) && b.IsPermanentlyUnavailable == false);
            }

            var bookList = await books.ToListAsync();

            var userId = HttpContext.Session.GetInt32("UserId");
            bool isUserLoggedIn = userId != null;

            foreach (var book in bookList)
            {
                var activeReservation = await _context.Reservations
                    .FirstOrDefaultAsync(r => r.BookId == book.BookId && r.ReservationEndDate > DateTime.Now);

                var activeLease = await _context.Leases
                    .FirstOrDefaultAsync(l => l.BookId == book.BookId && l.LeaseEndDate == null);

                book.IsReserved = activeReservation != null;
                book.IsLeased = activeLease != null;
            }

            ViewBag.IsUserLoggedIn = isUserLoggedIn;
            ViewBag.CurrentFilter = searchString;
            return View(bookList);
        }

        [HttpPost]
        public async Task<IActionResult> ReserveBook(int bookId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                TempData["Error"] = "You must be logged in to reserve a book.";
                return RedirectToAction("Login", "Account");
            }

            var book = await _context.Books.FirstOrDefaultAsync(b => b.BookId == bookId);

            if (book == null || book.IsPermanentlyUnavailable)
            {
                TempData["Error"] = "Book not found.";
                return RedirectToAction("List");
            }

            var existingReservation = await _context.Reservations
                .FirstOrDefaultAsync(r => r.BookId == bookId);
            var existingLease = await _context.Leases.FirstOrDefaultAsync(l => l.BookId == bookId && l.LeaseEndDate < DateTime.Now);

            if (existingReservation != null || existingLease != null)
            {
                TempData["Error"] = "This book is already leased or reserved.";
                return RedirectToAction("List");
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

            TempData["Success"] = "Book reserved successfully!";

            return RedirectToAction("List");
        }

        public async Task<IActionResult> ManageBooks()
        {
            if (!IsUserLibrarianAsync())
            {
                TempData["Error"] = "You do not have access to this page";
                return RedirectToAction("Index", "Home");
            }

            var books = await _context.Books.ToListAsync();
            return View(books);
        }

        public IActionResult AddBook()
        {
            if (!IsUserLibrarianAsync())
            {
                TempData["Error"] = "You do not have access to this page";
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddBook([Bind("Name, Author, Publisher, Date_of_publication, Price")] Book book)
        {
            if (!IsUserLibrarianAsync())
            {
                TempData["Error"] = "You do not have access to this page";
                return RedirectToAction("Index", "Home");
            }

            if (ModelState.IsValid)
            {
                _context.Books.Add(book);
                await _context.SaveChangesAsync();
                return RedirectToAction("ManageBooks");
            }
            return View(book);
        }

        public async Task<IActionResult> EditBook(int id)
        {
            if (!IsUserLibrarianAsync())
            {
                TempData["Error"] = "You do not have access to this page";
                return RedirectToAction("Index", "Home");
            }

            var book = await _context.Books.FirstOrDefaultAsync(b => b.BookId == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        [HttpPost]
        public async Task<IActionResult> EditBook(Book book)
        {
            if (!IsUserLibrarianAsync())
            {
                TempData["Error"] = "You do not have access to this page";
                return RedirectToAction("Index", "Home");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingBook = await _context.Books
                        .FirstOrDefaultAsync(b => b.BookId == book.BookId);

                    if (existingBook == null)
                    {
                        TempData["Error"] = "Book not found.";
                        return RedirectToAction("ManageBooks");
                    }

                    existingBook.Author = book.Author;
                    existingBook.Publisher = book.Publisher;
                    existingBook.DateOfPublication = book.DateOfPublication;
                    existingBook.Price = book.Price;
                    existingBook.IsPermanentlyUnavailable = book.IsPermanentlyUnavailable;
                    existingBook.Name = book.Name;

                    _context.Entry(existingBook).Property(b => b.RowVersion).OriginalValue = book.RowVersion;

                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Book updated successfully.";
                    return RedirectToAction("ManageBooks");
                }
                catch (DbUpdateConcurrencyException)
                {
                    TempData["Error"] = "The book data was modified by another user. Please reload and try again.";
                    return RedirectToAction("ManageBooks");
                }
            }

            TempData["Error"] = "Invalid/Missing book information.";
            return View(book);
        }




        public async Task<IActionResult> DeleteBook(int id)
        {
            if (!IsUserLibrarianAsync())
            {
                TempData["Error"] = "You do not have access to this page";
                return RedirectToAction("Index", "Home");
            }

            try
            {
                var book = await _context.Books
                    .Include(b => b.Leases)
                    .FirstOrDefaultAsync(b => b.BookId == id);

                if (book == null || book.IsPermanentlyUnavailable)
                {
                    TempData["Error"] = "Book not found or already marked as permanently unavailable.";
                    return RedirectToAction("ManageBooks");
                }

                var activeLease = book.Leases.Any(l => l.LeaseEndDate == null);
                if (activeLease)
                {
                    TempData["Error"] = "Book currently leased.";
                    return RedirectToAction("ManageBooks");
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
                TempData["Success"] = "Book deleted/marked as unavailable successfully.";
            }
            catch (DbUpdateConcurrencyException)
            {
                TempData["Error"] = "The book was modified or deleted by another user.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred while processing your request: {ex.Message}";
            }

            return RedirectToAction("ManageBooks");
        }


    }

}
