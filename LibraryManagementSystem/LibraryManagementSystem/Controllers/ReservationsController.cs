using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LibraryManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservationsController : ControllerBase
    {
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
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    return null;
                }
            }
            return null;
        }

        private readonly LibraryDbContext _context;
        private readonly IConfiguration _configuration;

        public ReservationsController(LibraryDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }


        // GET: api/reservations
        [HttpGet]
        [Authorize(Roles = "Librarian")]
        public async Task<ActionResult> GetReservations()
        {

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
        [Authorize(Roles = "User")]
        public async Task<ActionResult> DeleteReservation(int id)
        {
            var reservation = await _context.Reservations
                .FirstOrDefaultAsync(r => r.ReservationId == id);

            if (reservation == null)
            {
                return NotFound();
            }

            var userId = GetUserIdFromJwt();
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
        [Authorize(Roles = "Librarian")]
        public async Task<ActionResult> LeaseReservation(int id)
        {

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

                _context.Reservations.Remove(reservation);
                _context.Leases.Add(lease);
                await _context.SaveChangesAsync();
                return Ok("Book successfully leased from reservation.");
            
        }
    }
}
