using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Mvc;
using ItRoots.Business.Services;
using ItRoots.Data.Models;

namespace ItRoots.Web.Controllers
{
    [Route("[controller]/[action]")]
    public class UsersController : Controller
    {
        private readonly IUserService _userService;
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _config;

        public UsersController(IUserService userService, IEmailSender emailSender, IConfiguration config)
        {
            _userService = userService;
            _emailSender = emailSender;
            _config = config;
        }

        // GET: /Users/Register
        [HttpGet]
        public IActionResult Register()
        {
            // This will look for "Views/Users/Register.cshtml"
            return View();
        }

        // POST: /Users/Register
        [HttpPost]
        public async Task<IActionResult> Register(User model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // 1) Capture the recaptcha token from the form
            var recaptchaResponse = Request.Form["g-recaptcha-response"];

            // 2) Grab your secret key from config
            var secretKey = _config["Recaptcha:SecretKey"];

            // 3) Validate reCAPTCHA via Google
            var isValidCaptcha = await IsRecaptchaValidAsync(secretKey, recaptchaResponse);
            if (!isValidCaptcha)
            {
                ModelState.AddModelError("", "Invalid reCAPTCHA. Please try again.");
                return View(model);
            }

            // 4) Register user
            await _userService.RegisterAsync(model);

            // 5) Send verification email
            var verifyUrl = Url.Action(
                "VerifyEmail",
                "Users",
                new { token = model.VerificationToken },
                Request.Scheme);

            var body = $"Please click <a href='{verifyUrl}'>here</a> to verify your email.";
            await _emailSender.SendEmailAsync(model.Email, "Verify Your Email", body);

            ViewBag.Message = "Registered successfully. Check your email to verify.";
            // This will look for "Views/Users/RegisterSuccess.cshtml"
            return View("RegisterSuccess");
        }

        // GET: /Users/VerifyEmail?token=...
        [HttpGet]
        public async Task<IActionResult> VerifyEmail(Guid token)
        {
            var result = await _userService.VerifyEmailAsync(token);
            ViewBag.Message = result ? "Email verified. You can now login." : "Invalid or expired token.";
            // Looks for "Views/Users/VerifyEmail.cshtml"
            return View();
        }

        // GET: /Users/Login
        [HttpGet]
        public IActionResult Login()
        {
            // Looks for "Views/Users/Login.cshtml"
            return View();
        }

        // POST: /Users/Login
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("", "Username and Password required.");
                return View();
            }

            var user = await _userService.GetUserByUsernameAsync(username);
            if (user == null)
            {
                ModelState.AddModelError("", "User not found.");
                return View();
            }

            // Check password
            var hashed = _userService.HashPassword(password);
            if (user.PasswordHash != hashed)
            {
                ModelState.AddModelError("", "Invalid password.");
                return View();
            }

            if (!user.IsVerified)
            {
                ModelState.AddModelError("", "Email not verified. Check your inbox.");
                return View();
            }

            // Generate a simple JWT
            var secretKey = _config["JwtSettings:Key"] ?? "DefaultDevKey";
            var token = GenerateJwtToken(user, secretKey);

            ViewBag.Token = token;
            // Looks for "Views/Users/LoginSuccess.cshtml"
            return View("LoginSuccess");
        }

        public async Task<IActionResult> Dashboard()
        {
            var users = await _userService.GetAllUsersAsync();
            return View(users);
        }

        // GET: /Users/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound();

            // Looks for "Views/Users/Delete.cshtml"
            return View(user);
        }

        // POST: /Users/DeleteConfirmed/5
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _userService.DeleteUserAsync(id);
            // After deleting, redirect to Dashboard (was "Index" in older code)
            return RedirectToAction(nameof(Dashboard));
        }

        // ------------------------------------------------------------
        // Private helper methods
        // ------------------------------------------------------------
        private string GenerateJwtToken(User user, string secretKey)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private async Task<bool> IsRecaptchaValidAsync(string secretKey, string recaptchaResponse)
        {
            if (string.IsNullOrEmpty(recaptchaResponse))
                return false;

            var verifyUrl = $"https://www.google.com/recaptcha/api/siteverify?secret={secretKey}&response={recaptchaResponse}";

            using var client = new HttpClient();
            var result = await client.GetStringAsync(verifyUrl);

            // Minimal check; parse JSON if you need more robust validation
            return result.Contains("\"success\": true");
        }


        [HttpPost]
        public async Task<IActionResult> UpdateUser(ItRoots.Data.Models.User model)
        {
            if (!ModelState.IsValid)
            {
                // Optionally, set TempData or error messages.
                return RedirectToAction(nameof(Dashboard));
            }

            // Call the service layer update method
            await _userService.UpdateUserAsync(model);
            return RedirectToAction(nameof(Dashboard));
        }



    }
}
