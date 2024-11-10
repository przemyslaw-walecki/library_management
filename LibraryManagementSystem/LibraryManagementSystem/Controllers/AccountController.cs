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

        string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
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
        

        public IActionResult MyAccount()
        {

            if (!IsUserLoggedIn())
            {
                TempData["Error"] = "You do not have access to this page";
                return RedirectToAction("Index", "Home");
            }

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
            if (!IsUserLoggedIn())
            {
                TempData["Error"] = "You do not have access to this page";
                return RedirectToAction("Index", "Home");
            }
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

            var reservations = _context.Reservations.Where(r => r.UserId == userId.Value).ToList();
            var leases = _context.Leases.Where(l => l.UserId == userId.Value).ToList();

            _context.Reservations.RemoveRange(reservations);
            _context.Leases.RemoveRange(leases);


            _context.Users.Remove(user);
            _context.SaveChanges();

            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }

        [HttpPost]
        public IActionResult CancelReservation(int reservationId)
        {
            if (!IsUserLoggedIn())
            {
                TempData["Error"] = "You do not have access to this page";
                return RedirectToAction("Index", "Home");
            }
            var reservation = _context.Reservations.FirstOrDefault(r => r.ReservationId == reservationId);
            if (reservation == null)
            {
                TempData["Error"] = "Reservation not found.";
                return RedirectToAction("MyAccount");
            }

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

        public IActionResult EditUser(int id)
        {
            if (!IsUserLoggedIn() || !IsUserLibrarian())
            {
                TempData["Error"] = "You do not have access to this page";
                return RedirectToAction("Index", "Home");
            }
            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("Index");

            }
            user.Password = null;

            return View(user);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditUser(User user)
        {
            if (!IsUserLoggedIn() || !IsUserLibrarian())
            {
                TempData["Error"] = "You do not have access to this page";
                return RedirectToAction("Index", "Home");
            }
            if (ModelState.IsValid)
            {

                    if (!string.IsNullOrEmpty(user.Password))
                    {
                        var hashedPassword = HashPassword(user.Password);
                        user.Password = hashedPassword;
                    }

                    _context.Update(user);
                    _context.SaveChanges();

                    TempData["Success"] = "User updated successfully!";
                    return RedirectToAction("Index"); 
                }
            
            return View(user);
        }

        public IActionResult ManageUsers()
        {
            if (!IsUserLoggedIn() || !IsUserLibrarian())
            {
                TempData["Error"] = "You do not have access to this page";
                return RedirectToAction("Index", "Home");
            }
            var users = _context.Users.Where(u => u.IsLibrarian == false).ToList(); 
            return View(users); 
        }

    }
}

    




