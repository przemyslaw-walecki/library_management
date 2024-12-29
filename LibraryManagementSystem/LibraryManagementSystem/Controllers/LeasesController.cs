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

            return Ok(leases);
        }

        // GET: api/leases/5
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

            return Ok(lease);
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

            try
            {
                _context.Update(lease);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict("The lease was modified by another user. Please reload and try again.");
            }

            return Ok("Lease ended successfully!");
        }

    }
}
