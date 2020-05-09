using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SigortaTakipSistemi.Models;
using SigortaTakipSistemi.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Net.Mail;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using static SigortaTakipSistemi.Helpers.ProcessCollectionHelper;
using Microsoft.AspNetCore.Authorization;

namespace SigortaTakipSistemi.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppIdentityUser> _userManager;
        private readonly SignInManager<AppIdentityUser> _signInManager;
        private readonly IdentityContext _context;

        public AccountController(UserManager<AppIdentityUser> userManager, SignInManager<AppIdentityUser> signInManager, IdentityContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        [Route("account/register")]
        public IActionResult Register()
        {
            var model = new RegisterViewModel
            {

            };
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
                IsActive = true
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
        [ValidateAntiForgeryToken]
        [Route("account/login")]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            var result = await _signInManager.PasswordSignInAsync("", "", false, true);

            if (ModelState.IsValid)
            {
                var user = _userManager.FindByEmailAsync(model.Email).Result;

                if (user.IsActive != false)
                {
                    result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, true);
                    if (!result.Succeeded)
                    {
                        ViewBag.LoginError = "E-Posta veya Şifre hatalı girildi. Lütfen tekrar deneyiniz.";
                    }
                    else
                    {
                        return Redirect("~/Home");
                    }
                }
                else
                {
                    ViewBag.LoginError = "Hesabınız pasif durumdadır. Lütfen yöneticiniz ile iletişime geçiniz.";
                }
            }
            return View(model);
        }

        [Route("account/logout")]
        public async Task<IActionResult> LogOut()
        {
            await _signInManager.SignOutAsync();
            return Redirect("~/account/login");
        }

        [Route("account/forget-password")]
        public IActionResult ForgetPassword()
        {
            return View();
        }

        [HttpPost]
        [Route("account/forget-password")]
        public async Task<IActionResult> ForgetPassword(string email)
        {
            AppIdentityUser user = await _userManager.FindByEmailAsync(email);

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var resetLink = Url.Action("ResetPassword", "Account", new { token }, protocol: HttpContext.Request.Scheme);

            SendEmail(user.Email, resetLink);

            ViewBag.Message = "Şifre sıfırlama linki e-postanıza gönderilmiştir!";
            return View();
        }

        public void SendEmail(string userEmail, string resetLink)
        {
            MailMessage msg = new MailMessage();
            SmtpClient smtp = new SmtpClient("smtp.gmail.com");

            smtp.Port = 587;
            smtp.Host = "smtp.gmail.com";
            smtp.EnableSsl = true;
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = null;
            smtp.Credentials = new System.Net.NetworkCredential("banazsigorta@gmail.com", "Banaz26.,");

            msg.From = new MailAddress("banazsigorta@gmail.com", "Banaz Sigorta");

            msg.To.Add(userEmail);

            msg.Subject = "Şifrenizi Sıfırlayın";
            msg.Body = "Şifrenizi sıfırlamak için lütfen tıklayınız: " + resetLink;

            smtp.Send(msg);
        }

        [Route("account/reset-password")]
        public IActionResult ResetPassword(string token)
        {
            return View();
        }

        [HttpPost]
        [Route("account/reset-password")]
        public async Task<IActionResult> ResetPassword(RegisterViewModel model)
        {
            AppIdentityUser user = await _userManager.FindByEmailAsync(model.Email);

            IdentityResult result = _userManager.ResetPasswordAsync(user, model.Token, model.Password).Result;

            if (!result.Succeeded)
            {
                throw new TaskCanceledException("Şifrenizi sıfırlarken bir hata oluştu!"
                    + " Code: " + result.Errors.FirstOrDefault().Code
                    + " Description: " + result.Errors.FirstOrDefault().Description);
            }
            else
            {
                ViewBag.Message = "Şifreniz başarıyla yenilenmiştir!";
                return View();
            }
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Post()
        {
            var requestFormData = Request.Form;

            List<AppIdentityUser> users = await _context.Users
                .AsNoTracking()
                .ToListAsync();

            List<AppIdentityUser> listItems = ProcessCollection(users, requestFormData);

            var response = new PaginatedResponse<AppIdentityUser>
            {
                Data = listItems,
                Draw = int.Parse(requestFormData["draw"]),
                RecordsFiltered = users.Count,
                RecordsTotal = users.Count
            };

            return Ok(response);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(string email, string password)
        {
            var user = new AppIdentityUser
            {
                UserName = email,
                Email = email,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                foreach (var validateItem in result.Errors)
                    ModelState.AddModelError("", validateItem.Description);

                throw new TaskCanceledException("Kullanıcıyı oluştururken bir hata oluştu!"
                    + " Code: " + result.Errors.FirstOrDefault().Code
                    + " Description: " + result.Errors.FirstOrDefault().Description);
            }
            return View();
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return View("Error");
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return View("Error");
            }

            RegisterViewModel regUser = new RegisterViewModel
            {
                Email = user.Email,
                Password = ""
            };
            return View(regUser);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Edit(string email, string password)
        {
            RegisterViewModel model = new RegisterViewModel()
            {
                Email = email,
                Password = password
            };

            AppIdentityUser user = await _userManager.FindByEmailAsync(model.Email);

            var token = _userManager.GeneratePasswordResetTokenAsync(user).Result;

            IdentityResult result = _userManager.ResetPasswordAsync(user, token, model.Password).Result;

            if (!result.Succeeded)
            {
                throw new TaskCanceledException("Şifreyi güncellerken bir hata oluştu!"
                    + " Code: " + result.Errors.FirstOrDefault().Code
                    + " Description: " + result.Errors.FirstOrDefault().Description);
            }
            else
            {
                return View();
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Passive(string passiveUserId)
        {
            var user = await _context.Users.FindAsync(passiveUserId);
            if (user == null)
            {
                return View("Error");
            }

            user.IsActive = false;

            _context.Update(user);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Admin(string adminUserId)
        {
            var user = await _context.Users.FindAsync(adminUserId);
            if (user == null)
            {
                return View("Error");
            }

            var isUserAdmin = _context.UserRoles.Where(i => i.UserId == user.Id).ToList();

            if (isUserAdmin.Count != 0)
            {
                var result = await _userManager.RemoveFromRoleAsync(user, "Admin");
                user.IsAdmin = false;
                _context.SaveChanges();

                if (!result.Succeeded)
                {
                    return View("Error");
                }

                return Ok();
            }
            else
            {
                var result = await _userManager.AddToRoleAsync(user, "Admin");
                user.IsAdmin = true;
                _context.SaveChanges();

                if (!result.Succeeded)
                {
                    return View("Error");
                }

                return Ok();
            }
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
