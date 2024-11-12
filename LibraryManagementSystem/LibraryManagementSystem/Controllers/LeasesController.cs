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
    public class LeasesController : Controller
    {
        private readonly LibraryDbContext _context;

        public LeasesController(LibraryDbContext context)
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

        public async Task<IActionResult> Index()
        {
            if (!IsUserLoggedIn() || !IsUserLibrarian())
            {
                TempData["Error"] = "You do not have access to this page";
                return RedirectToAction("Index", "Home");
            }
            var libraryDbContext = _context.Leases.Include(l => l.Book).Include(l => l.User).Where(l => l.IsActive);
            return View(await libraryDbContext.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (!IsUserLoggedIn() || !IsUserLibrarian())
            {
                TempData["Error"] = "You do not have access to this page";
                return RedirectToAction("Index", "Home");
            }
            if (id == null || _context.Leases == null)
            {
                return NotFound();
            }

            var lease = await _context.Leases
                .Include(l => l.Book)
                .Include(l => l.User)
                .FirstOrDefaultAsync(m => m.LeaseId == id);
            if (lease == null)
            {
                return NotFound();
            }

            return View(lease);
        }

        public async Task<IActionResult> EndLease(int? id)
        {
            if (!IsUserLoggedIn() || !IsUserLibrarian())
            {
                TempData["Error"] = "You do not have access to this page";
                return RedirectToAction("Index", "Home");
            }
            if (id == null || _context.Leases == null)
            {
                return NotFound();
            }

            var lease = await _context.Leases
                .Include(l => l.Book)
                .Include(l => l.User)
                .FirstOrDefaultAsync(m => m.LeaseId == id);
            if (lease == null)
            {
                return NotFound();
            }

            return View(lease);
        }

        [HttpPost, ActionName("EndLease")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsUserLoggedIn() || !IsUserLibrarian())
            {
                TempData["Error"] = "You do not have access to this page";
                return RedirectToAction("Index", "Home");
            }
            if (_context.Leases == null)
            {
                return Problem("Entity set 'LibraryDbContext.Leases'  is null.");
            }
            var lease = await _context.Leases.FindAsync(id);
            if (lease != null)
            {
                lease.LeaseEndDate = DateTime.Now;
                lease.IsActive = false;
            }
            
            await _context.SaveChangesAsync();

            TempData["Success"] = "Lease ended successfully!";
            return RedirectToAction(nameof(Index));
        }

        private bool LeaseExists(int id)
        {
          return (_context.Leases?.Any(e => e.LeaseId == id)).GetValueOrDefault();
        }
    }
}
