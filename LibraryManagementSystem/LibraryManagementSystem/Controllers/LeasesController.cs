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
    public class LeasesController : ControllerBase
    {
        private readonly LibraryDbContext _context;

        public LeasesController(LibraryDbContext context)
        {
            _context = context;
        }

        // GET: api/leases
        [HttpGet]
        [Authorize(Roles = "Librarian")]
        public async Task<ActionResult> GetLeases()
        {
            var leases = await _context.Leases
                .Include(l => l.Book)
                .Include(l => l.User)
                .Where(l => l.IsActive)
                .ToListAsync();

            // Map leases to LeaseDto
            var leaseDtos = leases.Select(lease => new LeaseDto
            {
                LeaseId = lease.LeaseId,
                LeaseStartDate = lease.LeaseStartDate,
                LeaseEndDate = lease.LeaseEndDate,
                Book = new BookInfoDto
                {
                    BookId = lease.Book.BookId,
                    Name = lease.Book.Name,
                    Author = lease.Book.Author,
                    Publisher = lease.Book.Publisher,
                    DateOfPublication = lease.Book.DateOfPublication?.ToString("yyyy-MM-dd"),
                    Price = lease.Book.Price,
                },
                User = new UserDto
                {
                    UserId = lease.User.Id,
                    Username = lease.User.Username,
                    Email = lease.User.Email,
                    PhoneNumber = lease.User.PhoneNumber,
                    FirstName = lease.User.FirstName,
                    LastName = lease.User.LastName
                }
            }).ToList();

            return Ok(leaseDtos);
        }

        // GET: api/leases/{id}
        [HttpGet("{id}")]
        [Authorize(Roles = "Librarian")]
        public async Task<ActionResult> GetLease(int id)
        {
            var lease = await _context.Leases
                .Include(l => l.Book)
                .Include(l => l.User)
                .FirstOrDefaultAsync(m => m.LeaseId == id);

            if (lease == null)
            {
                return NotFound();
            }

            // Map lease to LeaseDto
            var leaseDto = new LeaseDto
            {
                LeaseId = lease.LeaseId,
                LeaseStartDate = lease.LeaseStartDate,
                LeaseEndDate = lease.LeaseEndDate,
                Book = new BookInfoDto
                {
                    BookId = lease.Book.BookId,
                    Name = lease.Book.Name,
                    Author = lease.Book.Author,
                    Publisher = lease.Book.Publisher,
                    DateOfPublication = lease.Book.DateOfPublication?.ToString("yyyy-MM-dd"),
                    Price = lease.Book.Price,
                },
                User = new UserDto
                {
                    UserId = lease.User.Id,
                    Username = lease.User.Username,
                    Email = lease.User.Email,
                    PhoneNumber = lease.User.PhoneNumber,
                    FirstName = lease.User.FirstName,
                    LastName = lease.User.LastName
                }
            };

            return Ok(leaseDto);
        }


        // PUT: api/leases/5/end
        [HttpPut("{id}/end")]
        [Authorize(Roles = "Librarian")]
        public async Task<ActionResult> EndLease(int id)
        {
            var lease = await _context.Leases
                .FirstOrDefaultAsync(m => m.LeaseId == id);

            if (lease == null)
            {
                return NotFound();
            }

            lease.LeaseEndDate = DateTime.Now;
            lease.IsActive = false;
            _context.Update(lease);
            await _context.SaveChangesAsync();
         

            return Ok("Lease ended successfully!");
        }

    }
}
