using LibraryManagementSystem.Models;
using LibraryManagementSystem.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace LibraryManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly LibraryDbContext _context;
        private readonly IConfiguration _configuration;

        public AccountController(LibraryDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Role, user.IsLibrarian ? "Librarian" : "User")
                }),
                Expires = DateTime.UtcNow.AddMinutes(10),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private int? GetUserIdFromJwt()
        {
            if (Request.Cookies.TryGetValue("AuthToken", out var token))
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
                try
                {
                    var claims = tokenHandler.ValidateToken(token, new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = false,
                        ValidateAudience = false
                    }, out _);

                    var userIdClaim = claims.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    return int.TryParse(userIdClaim, out var userId) ? userId : null;
                }
                catch
                {
                    return null;
                }
            }
            return null;
        }

        private static string HashPassword(string password)
        {
            var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(string Username, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == Username).ConfigureAwait(false);
            if (user == null || HashPassword(password) != user.Password)
            {
                return Unauthorized(new { message = "Invalid credentials." });
            }

            var token = GenerateJwtToken(user);
            Response.Cookies.Append("AuthToken", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddMinutes(10)
            });

            return Ok(new {token});

        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterDto user)
        {
            var detectUsernameConflict = await _context.Users.FirstOrDefaultAsync(u => u.Username == user.Username).ConfigureAwait(false);

            if (detectUsernameConflict != null)
            {
                return Conflict(new { message = "Username already exists." });
            }

            if (ModelState.IsValid)
            {
                var userToSave = new User
                {
                    Username = user.Username,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Password = HashPassword(user.Password),
                    Email = user.Email,
                    IsLibrarian = false,
                };
                if(!string.IsNullOrEmpty(user.PhoneNumber))
                {
                    userToSave.PhoneNumber = user.PhoneNumber;
                }

                _context.Add(userToSave);
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    return StatusCode(500, new { message = "An error occurred while saving the user." });
                }

                return Ok(new { message = "Registration successful." });
            }

            return BadRequest(new { message = "Invalid user data." });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("AuthToken");
            return Ok(new { message = "Logout successful." });
        }

        [HttpGet("myaccount")]
        public async Task<IActionResult> MyAccount()
        {
            var userId = GetUserIdFromJwt();
            if (userId == null)
            {
                return Unauthorized(new { message = "You are not logged in." });
            }

            var user = await _context.Users
                .Include(u => u.Reservations)
                .ThenInclude(r => r.Book)
                .Include(u => u.Leases)
                .ThenInclude(l => l.Book)
                .FirstOrDefaultAsync(u => u.Id == userId.Value);

            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            var userDto = new
            {
                user.FirstName,
                user.LastName,
                user.Email,
                user.PhoneNumber,
                Reservations = user.Reservations.Select(r => new ReservationDto
                {
                    ReservationId = r.ReservationId,
                    ReservationEndDate = r.ReservationEndDate,
                    Book = new BookInfoDto
                    {
                        BookId = r.Book.BookId,
                        Author = r.Book.Author,
                        Publisher = r.Book.Publisher,
                        DateOfPublication = r.Book.DateOfPublication?.ToString("yyyy-MM-dd"),
                        Price = r.Book.Price,
                        Name = r.Book.Name
                    }
                }).ToList(),
                Leases = user.Leases.Select(l => new LeaseDto
                {
                    LeaseId = l.LeaseId,
                    LeaseStartDate = l.LeaseStartDate,
                    LeaseEndDate = l.LeaseEndDate,
                    Book = new BookInfoDto
                    {
                        BookId = l.Book.BookId,
                        Author = l.Book.Author,
                        Publisher = l.Book.Publisher,
                        DateOfPublication = l.Book.DateOfPublication?.ToString("yyyy-MM-dd"),
                        Price = l.Book.Price,
                        Name = l.Book.Name
                    }
                }).ToList()
            };

            return Ok(userDto);
        }
        [HttpDelete]
        public async Task<IActionResult> DeleteAccount()
        {
            var userId = GetUserIdFromJwt();

            var user = await _context.Users
                .Include(u => u.Leases)
                .Include(u => u.Reservations)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            if (user.Leases.Any(l => l.IsActive))
            {
                return BadRequest(new { message = "User cannot be deleted because they have active leases." });
            }

            var userReservations = _context.Reservations.Where(r => r.UserId == userId);
            _context.Reservations.RemoveRange(userReservations);

            _context.Users.Remove(user);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return StatusCode(500, new { message = "An error occurred while deleting the user account." });
            }

            return Ok(new { message = "User account and all reservations have been deleted successfully." });
        }



    }
}
