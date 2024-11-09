using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using LibraryManagementSystem.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Text;
using Microsoft.AspNetCore.Identity;

namespace LibraryManagementSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly LibraryDbContext _context;

        public AccountController(LibraryDbContext context)
        {
            _context = context;
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View(new User());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string Username, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == Username);
            if(user ==null || HashPassword(password) != user.Password)
            {
                return RedirectToAction("InvalidCredentialsError");
            }

            HttpContext.Session.SetInt32(SessionData.SessionKeyUserId, user.Id);
            HttpContext.Session.SetInt32(SessionData.SessionKeyIsLibrarian, user.IsLibrarian ? 1 : 0);
            HttpContext.Session.SetString(SessionData.SessionKeyUsername, user.Username);


            return RedirectToAction("Index", "Home");
        }


        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View(new User());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([Bind("Id,Username,FirstName,LastName,Password,Email,PhoneNumber,IsLibrarian")] User user)
        {
            var detect_username_conflict = await _context.Users.FirstOrDefaultAsync(m => m.Username == user.Username);

            if (detect_username_conflict != null)
            {
                return RedirectToAction("UsernameAlreadyInUseError");
            }
            if (ModelState.IsValid) {
                user.Password = HashPassword(user.Password);
                _context.Add(user);
                await _context.SaveChangesAsync();

                HttpContext.Session.SetInt32(SessionData.SessionKeyUserId, user.Id);
                HttpContext.Session.SetInt32(SessionData.SessionKeyIsLibrarian, user.IsLibrarian ? 1 : 0);
                HttpContext.Session.SetString(SessionData.SessionKeyUsername, user.Username);

                return RedirectToAction("Index", "Home");
            }

              
            return View(user);

        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Remove(SessionData.SessionKeyUserId);
            HttpContext.Session.Remove(SessionData.SessionKeyIsLibrarian);
            HttpContext.Session.Remove(SessionData.SessionKeyUsername);
            Response.Cookies.Delete(".AspNetCore.Session");

            return RedirectToAction("Index", "Home");
        }
        string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }



        public IActionResult MyAccount()
        {
            

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var isLibrarian = HttpContext.Session.GetInt32(SessionData.SessionKeyIsLibrarian) == 1;

            ViewBag.IsLibrarian = isLibrarian;

            var user = _context.Users
                .Include(u => u.Reservations)
                .ThenInclude(r => r.Book)
                .FirstOrDefault(u => u.Id == userId);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return View(user);
        }

        [HttpPost]
        public IActionResult DeleteAccount()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = _context.Users.FirstOrDefault(u => u.Id == userId.Value);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Delete user's reservations and leases
            var reservations = _context.Reservations.Where(r => r.UserId == userId.Value).ToList();
            var leases = _context.Leases.Where(l => l.UserId == userId.Value).ToList();

            _context.Reservations.RemoveRange(reservations);
            _context.Leases.RemoveRange(leases);

            // Delete user account
            _context.Users.Remove(user);
            _context.SaveChanges();

            // Log the user out and redirect to login page
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }

        [HttpPost]
        public IActionResult CancelReservation(int reservationId)
        {
            var reservation = _context.Reservations.FirstOrDefault(r => r.ReservationId == reservationId);
            if (reservation == null)
            {
                TempData["Error"] = "Reservation not found.";
                return RedirectToAction("MyAccount");
            }

            // Check if the reservation belongs to the current user
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null || reservation.UserId != userId.Value)
            {
                TempData["Error"] = "You can only cancel your own reservations.";
                return RedirectToAction("MyAccount");
            }

            _context.Reservations.Remove(reservation);
            _context.SaveChanges();

            TempData["Success"] = "Reservation cancelled successfully.";
            return RedirectToAction("MyAccount", new User());
        }

    }

}

