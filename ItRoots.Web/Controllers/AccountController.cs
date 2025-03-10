using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using ItRoots.Business.Services;
using ItRoots.Data.Models;

namespace ItRoots.Web.Controllers
{
    [Route("[controller]/[action]")]
    public class AccountController : Controller
    {
        private readonly IUserService _userService;
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _config;

        public AccountController(IUserService userService, IEmailSender emailSender, IConfiguration config)
        {
            _userService = userService;
            _emailSender = emailSender;
            _config = config;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(User model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // 1. Capture the recaptcha token from the form
            var recaptchaResponse = Request.Form["g-recaptcha-response"];

            // 2. Grab your secret key from config
            var secretKey = _config["Recaptcha:SecretKey"];

            // 3. Validate with Google
            var isValidCaptcha = await IsRecaptchaValidAsync(secretKey, recaptchaResponse);
            if (!isValidCaptcha)
            {
                ModelState.AddModelError("", "Invalid reCAPTCHA. Please try again.");
                return View(model); // re-display the form
            }

            // 4. If valid, proceed with user registration
            await _userService.RegisterAsync(model);

            // 5. Send verification email
            var verifyUrl = Url.Action("VerifyEmail", "Account", new { token = model.VerificationToken }, Request.Scheme);
            var body = $"Please click <a href='{verifyUrl}'>here</a> to verify your email.";
            await _emailSender.SendEmailAsync(model.Email, "Verify Your Email", body);

            ViewBag.Message = "Registered successfully. Check your email to verify.";
            return View("RegisterSuccess");
        }

        [HttpGet]
        public async Task<IActionResult> VerifyEmail(Guid token)
        {
            var result = await _userService.VerifyEmailAsync(token);
            ViewBag.Message = result ? "Email verified. You can now login." : "Invalid or expired token.";
            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

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

            // Generate minimal JWT (no issuer/audience)
            var secretKey = _config["JwtSettings:Key"] ?? "DefaultDevKey";
            var token = GenerateJwtToken(user, secretKey);

            ViewBag.Token = token;
            return View("LoginSuccess");
        }

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

            // If you want to parse properly, you can do JSON deserialize,
            // but for quick 'Yes/No', a simple contains check often suffices:
            return result.Contains("\"success\": true");
        }
    }
}
