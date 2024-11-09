using Microsoft.AspNetCore.Mvc;
using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Http;
using LibraryManagementSystem.Data;
using Microsoft.EntityFrameworkCore;

namespace YourProject.Controllers
{
    public class BooksController : Controller
    {
        private readonly LibraryDbContext _context;

        public BooksController(LibraryDbContext context)
        {
            _context = context;
        }

        public IActionResult List()
        {
            var books = _context.Books
                 .ToList();

            var userId = HttpContext.Session.GetInt32("UserId");
            bool isUserLoggedIn = userId != null;

            foreach (var book in books)
            {

                var activeReservation = _context.Reservations
                    .FirstOrDefault(r => r.BookId == book.BookId && r.ReservationEndDate >= DateTime.Now);


                var activeLease = _context.Leases
                    .FirstOrDefault(l => l.BookId == book.BookId && l.LeaseEndDate >= DateTime.Now);

                book.IsReserved = activeReservation != null;
                book.IsLeased = activeLease != null;
            }

            ViewBag.IsUserLoggedIn = isUserLoggedIn;
            return View(books);
        }



        [HttpPost]
        public IActionResult ReserveBook(int bookId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                TempData["Error"] = "You must be logged in to reserve a book.";
                return RedirectToAction("Login", "Account");
            }

            var book = _context.Books.FirstOrDefault(b => b.BookId == bookId);

            if (book == null)
            {
                TempData["Error"] = "Book not found.";
                return RedirectToAction("List");
            }


            var existingReservation = _context.Reservations
                .FirstOrDefault(r => r.BookId == bookId && r.ReservationEndDate >= DateTime.Now);

            if (existingReservation != null)
            {
                TempData["Error"] = "This book is already reserved.";
                return RedirectToAction("List");
            }

 
            var newReservation = new Reservation
            {
                BookId = bookId,
                UserId = userId.Value, 
                ReservationDate = DateTime.Now,
                ReservationEndDate = DateTime.Now.AddDays(7)
            };

 
            _context.Reservations.Add(newReservation);
            _context.SaveChanges();

            TempData["Success"] = "Book reserved successfully!";



            return RedirectToAction("List");
        }

        public IActionResult ManageBooks()
        {
            var books = _context.Books.ToList();
            return View(books);
        }
        public IActionResult AddBook()
        {
            return View();
        }
        [HttpPost]
        public IActionResult AddBook(Book book)
        {
            if (ModelState.IsValid)
            {
                _context.Books.Add(book);
                _context.SaveChanges();
                return RedirectToAction("ManageBooks");
            }
            return View(book);
        }
        public IActionResult EditBook(int id)
        {
            var book = _context.Books.FirstOrDefault(b => b.BookId == id);
            if (book == null)
            {
                return NotFound();
            }
            return View(book);
        }

        [HttpPost]
        public IActionResult EditBook(Book book)
        {
            if (ModelState.IsValid)
            {
                _context.Books.Update(book);
                _context.SaveChanges();
                return RedirectToAction("ManageBooks");
            }
            return View(book);
        }
        public IActionResult DeleteBook(int id)
        {
            var book = _context.Books.FirstOrDefault(b => b.BookId == id);
            if (book != null)
            {
                _context.Books.Remove(book);
                _context.SaveChanges();
            }
            return RedirectToAction("ManageBooks");
        }
        [HttpGet]
        public IActionResult LeaseBook(int bookId)
        {
            var book = _context.Books.FirstOrDefault(b => b.BookId == bookId);
            if (book == null)
            {
                TempData["Error"] = "Book not found.";
                return RedirectToAction("ManageBooks", "Librarian");
            }
            
            // Get the reservation for the book (if any)
            var reservation = _context.Reservations
                .FirstOrDefault(r => r.BookId == bookId && r.ReservationEndDate >= DateTime.Now);

            // If there's no reservation, ask for the username to lease the book
            if (reservation == null)
            {
                ViewBag.ReservationStatus = "No reservation for this book.";
                return View(new LeaseBookViewModel { BookId = bookId, ReservedUsername = null });
            }

            // If there's a reservation, check if the username matches the reserved user
            var reservedUser = _context.Users.FirstOrDefault(u => u.Id == reservation.UserId);
            if (reservedUser != null)
            {
                ViewBag.ReservationStatus = "Reserved for " + reservedUser.Username;
                return View(new LeaseBookViewModel { BookId = bookId, ReservedUsername = reservedUser.Username });
            }

            TempData["Error"] = "Reservation data is invalid.";
            return RedirectToAction("ManageBooks", "Librarian");
        }

        [HttpPost]
        public IActionResult LeaseBook(LeaseBookViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Invalid input.";
                return RedirectToAction("ManageBooks", "Librarian");
            }

            var book = _context.Books.FirstOrDefault(b => b.BookId == model.BookId);
            if (book == null)
            {
                TempData["Error"] = "Book not found.";
                return RedirectToAction("ManageBooks", "Librarian");
            }

            // Check if the book has a reservation
            var reservation = _context.Reservations
                .FirstOrDefault(r => r.BookId == model.BookId && r.ReservationEndDate >= DateTime.Now);

            // If no reservation exists, simply create a lease for the user
            if (reservation == null)
            {
                var lease = new Lease
                {
                    BookId = model.BookId,
                    UserId = _context.Users.FirstOrDefault(u => u.Username == model.Username)?.Id ?? 0,
                    LeaseStartDate = DateTime.Now,
                    LeaseEndDate = DateTime.Now.AddDays(7)
                };

                _context.Leases.Add(lease);
                _context.SaveChanges();
                TempData["Success"] = "Book leased successfully!";
                return RedirectToAction("ManageBooks", "Librarian");
            }

            var reservedUser = _context.Users.FirstOrDefault(u => u.Id == reservation.UserId);
            if (reservedUser != null && reservedUser.Username == model.Username)
            {
                var lease = new Lease
                {
                    BookId = model.BookId,
                    UserId = reservedUser.Id,
                    LeaseStartDate = DateTime.Now,
                    LeaseEndDate = DateTime.Now.AddDays(7)
                };

                _context.Leases.Add(lease);
                reservation.ReservationEndDate = DateTime.Now;
                _context.SaveChanges();
                TempData["Success"] = "Book leased successfully to the reserved user!";
                return RedirectToAction("ManageBooks", "Librarian");
            }

            TempData["Error"] = "The username does not match the reservation.";
            return RedirectToAction("ManageBooks", "Librarian");
        }

    }
}
