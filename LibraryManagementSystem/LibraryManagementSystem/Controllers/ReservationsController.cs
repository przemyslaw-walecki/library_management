using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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

        private bool IsUserLoggedIn() =>
            HttpContext.Session.GetInt32(SessionData.SessionKeyUserId).HasValue;

        private bool IsUserLibrarian() =>
            HttpContext.Session.GetInt32(SessionData.SessionKeyIsLibrarian) == 1;

        // GET: Reservations
        public async Task<IActionResult> Index()
        {
            if (!IsUserLoggedIn() || !IsUserLibrarian())
            {
                TempData["Error"] = "You do not have access to this page";
                return RedirectToAction("Index", "Home");
            }

            var reservations = _context.Reservations
                .Include(r => r.Book)
                .Include(r => r.User)
                .Where(r => r.ReservationEndDate > DateTime.Now);

            return View(await reservations.ToListAsync());
        }

        // GET: Delete Reservation
        public async Task<IActionResult> Delete(int reservationId)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Book)
                .FirstOrDefaultAsync(r => r.ReservationId == reservationId);

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

        // POST: Delete Reservation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int reservationId)
        {
            var reservation = await _context.Reservations
                .FirstOrDefaultAsync(r => r.ReservationId == reservationId);

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null || reservation == null || reservation.UserId != userId.Value)
            {
                TempData["Error"] = "You can only cancel your own reservations.";
                return RedirectToAction("MyAccount", "Account");
            }

            _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Reservation canceled successfully.";
            return RedirectToAction("MyAccount", "Account");
        }

        // GET: Lease Reservation
        public async Task<IActionResult> Lease(int reservationId)
        {
            if (!IsUserLoggedIn() || !IsUserLibrarian())
            {
                TempData["Error"] = "You do not have access to this page";
                return RedirectToAction("Index", "Home");
            }

            var reservation = await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Book)
                .FirstOrDefaultAsync(r => r.ReservationId == reservationId);

            if (reservation == null || reservation.ReservationEndDate < DateTime.Now)
            {
                TempData["Error"] = "Error leasing from reservation.";
                return RedirectToAction(nameof(Index));
            }

            return View(reservation);
        }

        // POST: Lease Reservation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LeaseConfirmed(int reservationId)
        {
            if (!IsUserLoggedIn() || !IsUserLibrarian())
            {
                TempData["Error"] = "You do not have access to this page.";
                return RedirectToAction("Index", "Home");
            }

            var reservation = await _context.Reservations
                .FirstOrDefaultAsync(r => r.ReservationId == reservationId);

            if (reservation == null)
            {
                TempData["Error"] = "Reservation not found.";
                return RedirectToAction(nameof(Index));
            }

            var lease = new Lease
            {
                BookId = reservation.BookId,
                UserId = reservation.UserId,
                LeaseStartDate = DateTime.Now,
                LeaseEndDate = null,
                IsActive = true
            };

            try
            {
                _context.Reservations.Remove(reservation);
                _context.Leases.Add(lease);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Book successfully leased from reservation.";
            }
            catch (DbUpdateConcurrencyException)
            {
                TempData["Error"] = "The reservation was modified by another user. Please reload and try again.";
                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Reservation Details
        public async Task<IActionResult> Details(int? id)
        {
            if (!IsUserLoggedIn() || !IsUserLibrarian())
            {
                TempData["Error"] = "You do not have access to this page";
                return RedirectToAction("Index", "Home");
            }

            if (!id.HasValue)
            {
                return NotFound();
            }

            var reservation = await _context.Reservations
                .Include(r => r.Book)
                .Include(r => r.User)
                .FirstOrDefaultAsync(m => m.ReservationId == id);

            if (reservation == null)
            {
                return NotFound();
            }

            return View(reservation);
        }

        private bool ReservationExists(int id) =>
            _context.Reservations?.Any(e => e.ReservationId == id) ?? false;
    }
}
