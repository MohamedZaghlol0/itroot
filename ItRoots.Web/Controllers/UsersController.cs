using ItRoots.Business.Services;
using ItRoots.Data.Models;
using Microsoft.AspNetCore.Mvc;

namespace ItRoots.Web.Controllers
{
    public class UsersController : Controller
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }
        
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var users = await _userService.GetAllUsersAsync();
            return View(users);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        //public async Task<IActionResult> Create(User model)
        //{
        //    if (!ModelState.IsValid)
        //        return View(model);

        //    // Possibly hash password here or in service
        //    // model.PasswordHash = ...
        //    // or call: await _userService.RegisterAsync(model);

        //    await _userService.CreateUserAsync(model);
        //    return RedirectToAction(nameof(Index));
        //}

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }

        //[HttpPost]
        //public async Task<IActionResult> Edit(User model)
        //{
        //    if (!ModelState.IsValid)
        //        return View(model);

        //    await _userService.UpdateUserAsync(model);
        //    return RedirectToAction(nameof(Index));
        //}

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _userService.DeleteUserAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
