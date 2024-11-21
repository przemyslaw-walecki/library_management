using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Controllers
{
    public class ReservationsController : Controller
    {
        private readonly LibraryDbContext _context;

        public ReservationsController(LibraryDbContext context)
        {
            _context = context;
        }

        private bool IsUserLoggedIn()
        {
            var userId = HttpContext.Session.GetInt32(SessionData.SessionKeyUserId);
            return userId.HasValue;
        }

        private bool IsUserLibrarian()
        {
            var isLibrarian = HttpContext.Session.GetInt32(SessionData.SessionKeyIsLibrarian);
            return isLibrarian == 1;
        }

        // GET: Reservations
        public async Task<IActionResult> Index()
        {
            if(!IsUserLoggedIn()|| !IsUserLibrarian()) {
                TempData["Error"] = "You do not have access to this page";
                return RedirectToAction("Index", "Home");
            }
            var libraryDbContext = _context.Reservations.Include(r => r.Book).Include(r => r.User).Where(r => r.ReservationEndDate > DateTime.Now && r.IsActive);
            return View(await libraryDbContext.ToListAsync());
        }
        
        public IActionResult Delete(int reservationId)
        {
            var reservation = _context.Reservations.Include(r => r.Book)
                .FirstOrDefault(r => r.ReservationId == reservationId);

            if (reservation == null)
            {
                TempData["Error"] = "Reservation not found.";
                return RedirectToAction("MyAccount", "Account");
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null || reservation.UserId != userId.Value)
            {
                TempData["Error"] = "You can only cancel your own reservations.";
                return RedirectToAction("Index", "Home");
            }

            return View(reservation);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int reservationId)
        {

            var reservation = _context.Reservations.FirstOrDefault(r => r.ReservationId == reservationId);
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null || reservation == null || reservation.UserId != userId.Value)
            {
                TempData["Error"] = "You can only cancel your own reservations.";
                return RedirectToAction("MyAccount", "Account");
            }

            if (reservation != null)
            {
                _context.Reservations.Remove(reservation);
                _context.SaveChanges();
                TempData["Success"] = "Reservation canceled successfully.";
            }
            else
            {
                TempData["Error"] = "Reservation not found.";
            }

            return RedirectToAction("MyAccount", "Account");
        }

        public IActionResult Lease(int reservationId)
        {
            if(!IsUserLoggedIn()|| !IsUserLibrarian()) {
                TempData["Error"] = "You do not have access to this page";
                return RedirectToAction("Index", "Home");
            }
            var reservation = _context.Reservations.Include(r => r.User).Include(r => r.Book)
                .FirstOrDefault(r => r.ReservationId == reservationId);

            if (reservation == null || !reservation.IsActive || reservation.ReservationEndDate < DateTime.Now)
            {
                TempData["Error"] = "Error leasing from reservation.";
                return RedirectToAction(nameof(Index));
            }

            return View(reservation);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult LeaseConfirmed(int reservationId)
        {

            if (!IsUserLoggedIn() || !IsUserLibrarian())
            {
                TempData["Error"] = "You do not have access to this page.";
                return RedirectToAction("Index", "Home");
            }

            var reservation = _context.Reservations
                .FirstOrDefault(r => r.ReservationId == reservationId);

            if (reservation == null)
            {
                TempData["Error"] = "Reservation not found.";
                return RedirectToAction(nameof(Index));
            }

            reservation.IsActive = false;  
            reservation.ReservationEndDate = DateTime.Now;
            _context.Reservations.Update(reservation);

            var lease = new Lease
            {
                BookId = reservation.BookId,
                UserId = reservation.UserId,
                LeaseStartDate = DateTime.Now,
                LeaseEndDate = null,
                IsActive = true

            };

            _context.Leases.Add(lease);
            _context.SaveChanges();

            TempData["Success"] = "Book successfully leased from reservation.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (!IsUserLoggedIn() || !IsUserLibrarian())
            {
                TempData["Error"] = "You do not have access to this page";
                return RedirectToAction("Index", "Home");
            }
            if (id == null || _context.Reservations == null)
            {
                return NotFound();
            }

            var reservation = await _context.Reservations
                .Include(l => l.Book)
                .Include(l => l.User)
                .FirstOrDefaultAsync(m => m.ReservationId == id);
            if (reservation == null)
            {
                return NotFound();
            }

            return View(reservation);
        }

        private bool ReservationExists(int id)
        {
          return (_context.Reservations?.Any(e => e.ReservationId == id)).GetValueOrDefault();
        }
    }
}
