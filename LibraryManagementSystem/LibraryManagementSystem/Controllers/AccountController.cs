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
            else
            {
                
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



        public bool IsUserLoggedIn()
        {
            if (string.IsNullOrWhiteSpace(HttpContext.Session.GetInt32(SessionData.SessionKeyUserId).ToString()))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

    }

}

