using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;

namespace LibraryManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservationsController : ControllerBase
    {
        private readonly LibraryDbContext _context;

        public ReservationsController(LibraryDbContext context)
        {
            _context = context;
        }

        private bool IsUserLibrarian() =>
            HttpContext.User.IsInRole("Librarian");

        // GET: api/reservations
        [HttpGet]
        [Authorize]
        public async Task<ActionResult> GetReservations()
        {
            if (!HttpContext.User.Identity.IsAuthenticated || !IsUserLibrarian())
            {
                return Unauthorized("You do not have access to this page.");
            }

            var reservations = await _context.Reservations
                .Include(r => r.Book)
                .Include(r => r.User)
                .Where(r => r.ReservationEndDate > DateTime.Now)
                .ToListAsync();

            return Ok(reservations);
        }

        // GET: api/reservations/{id}
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult> GetReservation(int id)
        {
            if (!HttpContext.User.Identity.IsAuthenticated || !IsUserLibrarian())
            {
                return Unauthorized("You do not have access to this page.");
            }

            var reservation = await _context.Reservations
                .Include(r => r.Book)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.ReservationId == id);

            if (reservation == null)
            {
                return NotFound();
            }

            return Ok(reservation);
        }

        // DELETE: api/reservations/{id}
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult> DeleteReservation(int id)
        {
            var reservation = await _context.Reservations
                .FirstOrDefaultAsync(r => r.ReservationId == id);

            if (reservation == null)
            {
                return NotFound();
            }

            var userId = Convert.ToInt32(HttpContext.User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            if (reservation.UserId != userId)
            {
                return Unauthorized("You can only cancel your own reservations.");
            }

            _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();

            return Ok("Reservation canceled successfully.");
        }

        // PUT: api/reservations/{id}/lease
        [HttpPut("{id}/lease")]
        [Authorize]
        public async Task<ActionResult> LeaseReservation(int id)
        {
            if (!HttpContext.User.Identity.IsAuthenticated || !IsUserLibrarian())
            {
                return Unauthorized("You do not have access to this page.");
            }

            var reservation = await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Book)
                .FirstOrDefaultAsync(r => r.ReservationId == id);

            if (reservation == null || reservation.ReservationEndDate < DateTime.Now)
            {
                return BadRequest("Error leasing from reservation.");
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
                return Ok("Book successfully leased from reservation.");
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict("The reservation was modified by another user. Please reload and try again.");
            }
        }

        private bool ReservationExists(int id) =>
            _context.Reservations?.Any(e => e.ReservationId == id) ?? false;
    }
}
