using Microsoft.AspNetCore.Mvc;
using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Http;
using LibraryManagementSystem.Data;
using Microsoft.EntityFrameworkCore;
using System.IO.IsolatedStorage;

namespace YourProject.Controllers
{
    public class BooksController : Controller
    {
        private readonly LibraryDbContext _context;

        public BooksController(LibraryDbContext context)
        {
            _context = context;
        }
        private bool IsUserLibrarian()
        {
            var userId = HttpContext.Session.GetInt32(SessionData.SessionKeyUserId);
            var isLibrarian = HttpContext.Session.GetInt32(SessionData.SessionKeyIsLibrarian);
            if (!userId.HasValue || !isLibrarian.HasValue || isLibrarian == 0)
            {
                return false;
            }
            return true;
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
                ReservationEndDate = DateTime.Now.Date.AddDays(2).AddSeconds(-1)  
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
            if (!IsUserLibrarian())
            {
                TempData["Error"] = "You do not have access to this page";
                return RedirectToAction("Index", "Home");
            }
            return View();
        }
        [HttpPost]
        public IActionResult AddBook(Book book)
        {
            if (!IsUserLibrarian())
            {
                TempData["Error"] = "You do not have access to this page";
                return RedirectToAction("Index", "Home");
            }
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
            if (!IsUserLibrarian())
            {
                TempData["Error"] = "You do not have access to this page";
                return RedirectToAction("Index", "Home");
            }
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
            if (!IsUserLibrarian())
            {
                TempData["Error"] = "You do not have access to this page";
                return RedirectToAction("Index", "Home");
            }
            if (ModelState.IsValid)
            {
                _context.Books.Update(book);
                _context.SaveChanges();
                TempData["Success"] = "Book updated successfully.";
                return RedirectToAction("ManageBooks");

            }
            TempData["Error"] = "Invalid/Missing book information.";
            return View(book);
        }

        public IActionResult DeleteBook(int id)
        {
            if (!IsUserLibrarian())
            {
                TempData["Error"] = "You do not have access to this page";
                return RedirectToAction("Index", "Home");
            }
            var book = _context.Books.FirstOrDefault(b => b.BookId == id);
            if (book != null && book.IsPermanentlyUnavailable == false)
            {
                book.IsPermanentlyUnavailable = true;
                _context.SaveChanges();
                TempData["Success"] = "Book marked as permanently unavailable successfully.";
            }
            else
            {
                TempData["Error"] = "Book not found.";
            }
            
            return RedirectToAction("ManageBooks");
        }
       

    }
}
