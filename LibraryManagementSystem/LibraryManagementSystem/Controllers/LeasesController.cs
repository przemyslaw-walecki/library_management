using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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

        private bool IsUserLoggedIn() =>
            HttpContext.Session.GetInt32(SessionData.SessionKeyUserId).HasValue;

        private bool IsUserLibrarian() =>
            HttpContext.Session.GetInt32(SessionData.SessionKeyIsLibrarian) == 1;

        public async Task<IActionResult> Index()
        {
            if (!IsUserLoggedIn() || !IsUserLibrarian())
            {
                TempData["Error"] = "You do not have access to this page";
                return RedirectToAction("Index", "Home");
            }

            var leases = _context.Leases
                .Include(l => l.Book)
                .Include(l => l.User)
                .Where(l => l.IsActive);

            return View(await leases.ToListAsync());
        }

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

            var lease = await _context.Leases
                .Include(l => l.Book)
                .Include(l => l.User)
                .FirstOrDefaultAsync(m => m.LeaseId == id.Value);

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

            if (!id.HasValue)
            {
                return NotFound();
            }

            var lease = await _context.Leases
                .Include(l => l.Book)
                .Include(l => l.User)
                .FirstOrDefaultAsync(m => m.LeaseId == id.Value);

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

            var lease = await _context.Leases.FindAsync(id);
            if (lease != null)
            {
                try
                {
                    lease.LeaseEndDate = DateTime.Now;
                    lease.IsActive = false;

                    _context.Update(lease);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Lease ended successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    TempData["Error"] = "The lease was modified by another user. Please reload and try again.";
                    return RedirectToAction(nameof(Index));
                }
            }

            return RedirectToAction(nameof(Index));
        }

        private bool LeaseExists(int id) =>
            _context.Leases?.Any(e => e.LeaseId == id) ?? false;
    }
}
