using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace SigortaTakipSistemi.Controllers
{
    public class TestController : Controller
    {
        public IActionResult ServerSidePagination()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ServerSidePagination(int asd)
        {
            return View();
        }
    }
}