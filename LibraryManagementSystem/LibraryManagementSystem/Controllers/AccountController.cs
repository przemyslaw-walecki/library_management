using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using LibraryManagementSystem.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

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

        private string HashPassword(string password)
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
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == Username).ConfigureAwait(false);
            if (user == null || HashPassword(password) != user.Password)
            {
                TempData["Error"] = "Invalid Credentials";
                return RedirectToAction(nameof(Login));
            }

            // Store session data in a thread-safe manner
            HttpContext.Session.SetInt32(SessionData.SessionKeyUserId, user.Id);
            HttpContext.Session.SetInt32(SessionData.SessionKeyIsLibrarian, user.IsLibrarian ? 1 : 0);
            HttpContext.Session.SetString(SessionData.SessionKeyUsername, user.Username);

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View(new User());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([Bind("Id,Username,FirstName,LastName,Password,Email,PhoneNumber,IsLibrarian")] User user)
        {
            var detect_username_conflict = await _context.Users.FirstOrDefaultAsync(m => m.Username == user.Username).ConfigureAwait(false);

            if (detect_username_conflict != null)
            {
                TempData["Error"] = "Username already exists.";
                return RedirectToAction(nameof(Register));
            }

            if (ModelState.IsValid)
            {
                user.Password = HashPassword(user.Password);
                user.IsLibrarian = false;
                _context.Add(user);
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    TempData["Error"] = "An error occurred while saving the user.";
                    return View(user);
                }

                HttpContext.Session.SetInt32(SessionData.SessionKeyUserId, user.Id);
                HttpContext.Session.SetInt32(SessionData.SessionKeyIsLibrarian, user.IsLibrarian ? 1 : 0);
                HttpContext.Session.SetString(SessionData.SessionKeyUsername, user.Username);

                return RedirectToAction("Index", "Home");
            }
            TempData["Error"] = "Missing Information";

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
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
                .Include(u => u.Leases)
                .ThenInclude(r => r.Book)
                .FirstOrDefault(u => u.Id == userId);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAccount()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                TempData["Error"] = "You do not have access to this page";
                return RedirectToAction("Index", "Home");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId.Value).ConfigureAwait(false);
            if (user == null)
            {
                TempData["Error"] = "You do not have access to this page";
                return RedirectToAction("Index", "Home");
            }

            var leases = await _context.Leases.Where(l => l.UserId == userId.Value && l.IsActive).ToListAsync().ConfigureAwait(false);
            if (leases.Count != 0)
            {
                TempData["Error"] = "You can't delete an account with active leases.";
                return RedirectToAction(nameof(MyAccount));
            }

            var reservations = await _context.Reservations.Where(r => r.UserId == userId.Value).ToListAsync().ConfigureAwait(false);
            var leases_history = await _context.Leases.Where(l => l.UserId == userId.Value).ToListAsync().ConfigureAwait(false);

            _context.Leases.RemoveRange(leases_history);
            _context.Reservations.RemoveRange(reservations);
            _context.Users.Remove(user);
            await _context.SaveChangesAsync().ConfigureAwait(false);

            HttpContext.Session.Clear();

            TempData["Success"] = "Account deleted successfully.";
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> EditUser(int id)
        {
            if (!IsUserLoggedIn() || !IsUserLibrarian())
            {
                TempData["Error"] = "You do not have access to this page";
                return RedirectToAction("Index", "Home");
            }
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("Index");
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(User user)
        {
            if (!IsUserLoggedIn() || !IsUserLibrarian())
            {
                TempData["Error"] = "You do not have access to this page";
                return RedirectToAction("Index", "Home");
            }
            if (ModelState.IsValid)
            {
                try
                {
                    var existingUser = await _context.Users
                        .FirstOrDefaultAsync(u => u.Id == user.Id);

                    if (existingUser == null)
                    {
                        TempData["Error"] = "User not found.";
                        return RedirectToAction("ManageUsers");
                    }

                    existingUser.Email = user.Email;
                    existingUser.PhoneNumber = user.PhoneNumber;


                    _context.Entry(existingUser).Property(u => u.Version).OriginalValue = user.Version;

                    await _context.SaveChangesAsync();

                    TempData["Success"] = "User updated successfully.";
                    return RedirectToAction("ManageUsers");
                }
                catch (DbUpdateConcurrencyException)
                {
                    TempData["Error"] = "The User data was modified by another user. Please reload and try again.";
                    return RedirectToAction("ManageUsers");
                }
            }

            foreach (var state in ModelState)
            {
                foreach (var error in state.Value.Errors)
                {
                    Console.WriteLine($"Field: {state.Key}, Error: {error.ErrorMessage}");
                }
            }

            TempData["Error"] = "Invalid/Missing User information.";
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
