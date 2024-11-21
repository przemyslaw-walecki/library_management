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
                TempData["Error"] = "Invalid Credentials";
                return RedirectToAction(nameof(Login));
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
                TempData["Error"] = "Username already exists.";
                return RedirectToAction(nameof(Register));
            }
            if (ModelState.IsValid) {
                user.Password = HashPassword(user.Password);
                user.IsLibrarian = false;
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
        public IActionResult DeleteAccount()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                TempData["Error"] = "You do not have access to this page";
                return RedirectToAction("Index", "Home");
            }
            

            var user = _context.Users.FirstOrDefault(u => u.Id == userId.Value);
            if (user == null)
            {
                TempData["Error"] = "You do not have access to this page";
                return RedirectToAction("Index", "Home");
            }

            var leases = _context.Leases.Where(l => l.UserId == userId.Value && l.IsActive).ToList();
            if (leases.Count != 0)
            {
                TempData["Error"] = "You can't delete an account with active leases.";
                return RedirectToAction(nameof(MyAccount));
            }

            var reservations = _context.Reservations.Where(r => r.UserId == userId.Value).ToList();
            var leases_history = _context.Leases.Where(l => l.UserId == userId.Value).ToList();

            _context.Leases.RemoveRange(leases_history);
            _context.Reservations.RemoveRange(reservations);

            _context.Users.Remove(user);
            _context.SaveChanges();

            HttpContext.Session.Clear();

            TempData["Success"] = "Account deleted successfully.";
            return RedirectToAction("Index", "Home");
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

    




