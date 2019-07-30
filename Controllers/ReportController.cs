using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
            ViewBag.InsuranceCompanies = new SelectList(_context.InsuranceCompanies.OrderBy(x => x.Name), "Id", "Name");

            return View();
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProfitReportResult(ReportViewModel reportViewModel)
        {
            if (ModelState.IsValid)
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
                            && reportViewModel.InsuranceCompany.Contains(i.InsuranceCompanyId))
                .AsNoTracking()
                .ToListAsync();

                FakeSession.Instance.Obj = JsonConvert.SerializeObject(_context.Insurances
                    .Where(i => i.IsActive == true
                    && i.InsuranceStartDate >= reportViewModel.StartDate
                    && i.InsuranceStartDate <= reportViewModel.FinishDate
                    && reportViewModel.InsuranceCompany.Contains(i.InsuranceCompanyId)));

                return View(insurances);
            }
            return RedirectToAction("ProfitReport");
        }

        public ActionResult AllProfitReport()
        {
            return View();
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AllProfitReportResult(ReportViewModel reportViewModel)
        {
            if (ModelState.IsValid)
            {
                List<Insurances> insurances = await _context.Insurances
                .Include(cu => cu.Customer)
                .Include(c => c.CarModel)
                .Include(cb => cb.CarModel.CarBrand)
                .Include(pn => pn.InsurancePolicy)
                .Include(pc => pc.InsuranceCompany)
                .Where(i => i.IsActive == true
                            && i.InsuranceStartDate >= reportViewModel.StartDate
                            && i.InsuranceStartDate <= reportViewModel.FinishDate)
                .AsNoTracking()
                .ToListAsync();

                FakeSession.Instance.Obj = JsonConvert.SerializeObject(_context.Insurances
                    .Where(i => i.IsActive == true
                    && i.InsuranceStartDate >= reportViewModel.StartDate
                    && i.InsuranceStartDate <= reportViewModel.FinishDate));

                return View(insurances);
            }
            return RedirectToAction("AllProfitReport");
        }

        public ActionResult GeneralInsuranceReport()
        {
            return View();
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GeneralInsuranceReportResult(ReportViewModel reportViewModel)
        {
            if (ModelState.IsValid)
            {
                var insurancesGroup = await _context.Insurances
                    .Include(ic => ic.InsuranceCompany)
                    .Include(ip => ip.InsurancePolicy)
                    .Where(i => i.IsActive == true
                        && i.InsuranceStartDate >= reportViewModel.StartDate
                        && i.InsuranceStartDate <= reportViewModel.FinishDate)
                    .GroupBy(i => new
                    {
                        InsuranceCompanyName = i.InsuranceCompany.Name,
                        InsurancePolicyName = i.InsurancePolicy.Name
                    })
                    .Select(i => new GeneralReportViewModel
                    {
                        InsuranceCompanyName = i.Key.InsuranceCompanyName,
                        InsurancePolicyName = i.Key.InsurancePolicyName,
                        Count = i.Count()
                    })
                    .OrderBy(i => i.InsuranceCompanyName)
                    .ToListAsync();

                return View(insurancesGroup);
            }

            return RedirectToAction("GeneralInsuranceReport");
        }

        public ActionResult InsurancePaymentTypeReport()
        {
            return View();
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InsurancePaymentTypeReportResult(ReportViewModel reportViewModel)
        {
            if (ModelState.IsValid)
            {
                var insurancesGroup = await _context.Insurances
                    .Include(ic => ic.InsuranceCompany)
                    .Include(ip => ip.InsurancePolicy)
                    .Where(i => i.IsActive == true
                        && i.InsuranceStartDate >= reportViewModel.StartDate
                        && i.InsuranceStartDate <= reportViewModel.FinishDate
                        && reportViewModel.InsurancePaymentType.Contains(i.InsurancePaymentType))
                    .GroupBy(i => new
                    {
                        InsurancePaymentTypeEnum = i.InsurancePaymentType
                    })
                    .Select(i => new GeneralReportViewModel
                    {
                        InsurancePaymentType = i.Key.InsurancePaymentTypeEnum,
                        Count = i.Count()
                    })
                    .OrderBy(i => i.InsurancePaymentType)
                    .ToListAsync();

                foreach (var item in insurancesGroup)
                {
                    if (item.InsurancePaymentType == 0)
                    {
                        item.InsurancePaymentTypeName = Helpers.EnumExtensionsHelper.GetDisplayName(Insurances.InsurancePaymentTypeEnum.CASH);
                    }
                    if (item.InsurancePaymentType == 1)
                    {
                        item.InsurancePaymentTypeName = Helpers.EnumExtensionsHelper.GetDisplayName(Insurances.InsurancePaymentTypeEnum.CREDIT_CARD);
                    }
                    if (item.InsurancePaymentType == 2)
                    {
                        item.InsurancePaymentTypeName = Helpers.EnumExtensionsHelper.GetDisplayName(Insurances.InsurancePaymentTypeEnum.UNPAID);
                    }
                }

                return View(insurancesGroup);
            }

            return RedirectToAction("GeneralInsuranceReport");
        }
    }
}