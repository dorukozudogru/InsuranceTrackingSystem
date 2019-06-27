using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SigortaTakipSistemi.Models;
using Microsoft.AspNetCore.Identity;
using System.Linq;

namespace SigortaTakipSistemi.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppIdentityUser> _userManager;
        private readonly SignInManager<AppIdentityUser> _signInManager;

        public AccountController(UserManager<AppIdentityUser> userManager, SignInManager<AppIdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        //public async Task<IActionResult> Index()
        //{
        //    return View(await _context.LoginViewModel.ToListAsync());
        //}

        //public async Task<IActionResult> Details(Guid? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var loginViewModel = await _context.LoginViewModel
        //        .FirstOrDefaultAsync(m => m.Id == id);
        //    if (loginViewModel == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(loginViewModel);
        //}

        //public IActionResult Create()
        //{
        //    return View();
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("Id,Username,Password,RememberMe,ReturnUrl")] LoginViewModel loginViewModel)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        loginViewModel.Id = Guid.NewGuid();
        //        _context.Add(loginViewModel);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }
        //    return View(loginViewModel);
        //}

        //public async Task<IActionResult> Edit(Guid? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var loginViewModel = await _context.LoginViewModel.FindAsync(id);
        //    if (loginViewModel == null)
        //    {
        //        return NotFound();
        //    }
        //    return View(loginViewModel);
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(Guid id, [Bind("Id,Username,Password,RememberMe,ReturnUrl")] LoginViewModel loginViewModel)
        //{
        //    if (id != loginViewModel.Id)
        //    {
        //        return NotFound();
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            _context.Update(loginViewModel);
        //            await _context.SaveChangesAsync();
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!LoginViewModelExists(loginViewModel.Id))
        //            {
        //                return NotFound();
        //            }
        //            else
        //            {
        //                throw;
        //            }
        //        }
        //        return RedirectToAction(nameof(Index));
        //    }
        //    return View(loginViewModel);
        //}

        //public async Task<IActionResult> Delete(Guid? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var loginViewModel = await _context.LoginViewModel
        //        .FirstOrDefaultAsync(m => m.Id == id);
        //    if (loginViewModel == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(loginViewModel);
        //}

        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(Guid id)
        //{
        //    var loginViewModel = await _context.LoginViewModel.FindAsync(id);
        //    _context.LoginViewModel.Remove(loginViewModel);
        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}

        //private bool LoginViewModelExists(Guid id)
        //{
        //    return _context.LoginViewModel.Any(e => e.Id == id);
        //}

        [Route("account/register")]
        public IActionResult Register()
        {
            var model = new RegisterViewModel
            {

            };
            return View(model);
        }

        [Route("account/dashboard")]
        public IActionResult Dashboard()
        {
            LoginViewModel model = new LoginViewModel();
            model.Email = this.User.Claims.FirstOrDefault().Subject.Name;
            return View(model);
        }

        [HttpPost]
        [Route("account/register")]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            var user = new AppIdentityUser
            {
                UserName = model.Email,
                Email = model.Email,
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                foreach (var validateItem in result.Errors)
                    ModelState.AddModelError("", validateItem.Description);

                return View(model);
            }

            return Redirect("~/account/login");
        }

        [Route("account/login")]
        public IActionResult Login()
        {
            var model = new LoginViewModel
            {

            };
            return View(model);
        }

        [HttpPost]
        [Route("account/login")]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, true);
            if (result.Succeeded)
            {
                return Redirect("~/account/dashboard");
            }

            return View(model);
        }

        [Route("account/log-out")]
        public async Task<IActionResult> LogOut()
        {
            await _signInManager.SignOutAsync();
            return Redirect("~/account/login");
        }

        [Route("account/access-denied")]
        public IActionResult AccessDenied()
        {
            return View();
        }

    }
}
