using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SigortaTakipSistemi.Models;
using SigortaTakipSistemi.Models.ViewModels;
using static SigortaTakipSistemi.Controllers.InsuranceController;

namespace SigortaTakipSistemi.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        private readonly IdentityContext _context;

        public ReportController(IdentityContext context)
        {
            _context = context;
        }

        public ActionResult ProfitReport()
        {
            ViewBag.InsuranceCompanies = new SelectList(_context.InsuranceCompanies, "Id", "Name");

            return View();
        }

        public async Task<IActionResult> ProfitReportResult(ReportViewModel reportViewModel)
        {
            List<Insurances> insurances = await _context.Insurances
                .Include(cu => cu.Customer)
                .Include(c => c.CarModel)
                .Include(cb => cb.CarModel.CarBrand)
                .Include(pn => pn.InsurancePolicy)
                .Include(pc => pc.InsuranceCompany)
                .Where(i => i.IsActive == true
                            && i.InsuranceStartDate >= reportViewModel.StartDate
                            && i.InsuranceStartDate <= reportViewModel.FinishDate
                            && i.InsuranceCompanyId == reportViewModel.InsuranceCompany)
                .AsNoTracking()
                .ToListAsync();

            FakeSession.Instance.Obj = JsonConvert.SerializeObject(_context.Insurances
                .Where(i => i.IsActive == true
                && i.InsuranceStartDate >= reportViewModel.StartDate
                && i.InsuranceStartDate <= reportViewModel.FinishDate
                && i.InsuranceCompanyId == reportViewModel.InsuranceCompany));

            return View(insurances);
        }
    }
}