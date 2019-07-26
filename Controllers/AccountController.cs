using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SigortaTakipSistemi.Models;
using SigortaTakipSistemi.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Security.Claims;
using System.Net.Mail;

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
                UserName = model.Email.Split("@")[0],
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
        [ValidateAntiForgeryToken]
        [Route("account/login")]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            var result = await _signInManager.PasswordSignInAsync("", "", false, true);

            if (model.Email == null && model.Password == null)
            {
#if DEBUG
                model = new LoginViewModel
                {
                    Email = "dorukozudogru@gmail.com",
                    Password = "QWEqwe.1"
                };
                
                result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, true);
                return Redirect("~/Home");
#endif
            }
            if (ModelState.IsValid)
            {
                result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, true);
                if (result.Succeeded)
                {
                    return Redirect("~/Home");
                }
                else
                {
                    ViewBag.LoginError = "E-Posta veya Şifre hatalı girildi. Lütfen tekrar deneyiniz.";
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
            AppIdentityUser user = await _userManager.FindByNameAsync(email);

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
            smtp.Credentials = new System.Net.NetworkCredential("dorukozudogru.oplog@gmail.com", "doruk.,.,23");

            msg.From = new MailAddress("dorukozudogru.oplog@gmail.com", "Banaz Sigorta");

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
            AppIdentityUser user = await _userManager.FindByNameAsync(model.Email);

            IdentityResult result = _userManager.ResetPasswordAsync(user, model.Token, model.Password).Result;

            if (result.Succeeded)
            {
                ViewBag.Message = "Şifreniz başarıyla yenilenmiştir!";
                return View();
            }
            else
            {
                throw new TaskCanceledException("Şifrenizi sıfırlarken bir hata oluştu!"
                    + " Code: " + result.Errors.FirstOrDefault().Code
                    + " Description: " + result.Errors.FirstOrDefault().Description);
            }
        }
    }
}
