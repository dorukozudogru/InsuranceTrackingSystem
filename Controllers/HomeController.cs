using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SigortaTakipSistemi.Models;
using SigortaTakipSistemi.Models.ViewModels;

namespace SigortaTakipSistemi.Controllers
{
    public class HomeController : Controller
    {
        private readonly IdentityContext _context;

        public HomeController(IdentityContext context)
        {
            _context = context;
        }

        [Authorize]
        public IActionResult Index()
        {
            ViewBag.InsuranceCount = _context.Insurances.Count();
            ViewBag.CustomerCount = _context.Customers.Count();
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var pathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            Exception exception = pathFeature?.Error;
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier, RequestMessage = exception.Message.ToString() });
        }
    }
}
