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
using SigortaTakipSistemi.Helpers;
using SigortaTakipSistemi.Models;
using SigortaTakipSistemi.Models.ViewModels;
using static SigortaTakipSistemi.Controllers.InsuranceController;
using static SigortaTakipSistemi.Helpers.ProcessCollectionHelper;

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

        public ActionResult ProfitReportWithInsurancePolicyType()
        {
            ViewBag.InsurancePolicies = new SelectList(_context.InsurancePolicies.OrderBy(x => x.Name), "Id", "Name");
            ViewBag.InsuranceCompanies = new SelectList(_context.InsuranceCompanies.OrderBy(x => x.Name), "Id", "Name");
            return View();
        }

        public ActionResult AllProfitReport()
        {
            return View();
        }

        public ActionResult GeneralInsuranceReport()
        {
            return View();
        }

        public ActionResult InsurancePaymentTypeReport()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Post(string reportType, DateTime startDate, DateTime finishDate, string insuranceCompanies, string insurancePolicies, bool isCancelledIncluded)
        {
            var requestFormData = Request.Form;

            List<Insurances> insurances = await _context.Insurances
                .Include(cu => cu.Customer)
                .Include(c => c.CarModel)
                .Include(cb => cb.CarModel.CarBrand)
                .Include(pn => pn.InsurancePolicy)
                .Include(pc => pc.InsuranceCompany)
                .Where(i => i.IsActive == !isCancelledIncluded
                            && i.InsuranceStartDate >= startDate
                            && i.InsuranceStartDate <= finishDate)
                .AsNoTracking()
                .ToListAsync();

            if (reportType == "ins")
            {
                if (!string.IsNullOrEmpty(insuranceCompanies))
                {
                    string[] insuranceCompaniesList = insuranceCompanies.Split(",");
                    insurances = insurances.Where(i => insuranceCompaniesList.Contains(i.InsuranceCompanyId.ToString())).ToList();

                    if (!string.IsNullOrEmpty(insurancePolicies))
                    {
                        string[] insurancePoliciesList = insurancePolicies.Split(",");
                        insurances = insurances.Where(i => insurancePoliciesList.Contains(i.InsurancePolicyId.ToString())).ToList();
                    }
                }
            }

            insurances = GetAllEnumNamesHelper.GetEnumName(insurances);

            FakeSession.Instance.Obj = JsonConvert.SerializeObject(insurances);

            List<Insurances> listItems = ProcessCollection(insurances, requestFormData);

            var response = new PaginatedResponse<Insurances>
            {
                Data = listItems,
                Draw = int.Parse(requestFormData["draw"]),
                RecordsFiltered = insurances.Count,
                RecordsTotal = insurances.Count
            };

            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> PostTotalAmount(string reportType, DateTime startDate, DateTime finishDate, string insuranceCompanies, string insurancePolicies, bool isCancelledIncluded)
        {
            List<Insurances> insurances = await _context.Insurances
                .Where(i => i.IsActive == !isCancelledIncluded
                            && i.InsuranceStartDate >= startDate
                            && i.InsuranceStartDate <= finishDate)
                .ToListAsync();

            if (reportType == "ins")
            {
                if (!string.IsNullOrEmpty(insuranceCompanies))
                {
                    string[] insuranceCompaniesList = insuranceCompanies.Split(",");
                    insurances = insurances.Where(i => insuranceCompaniesList.Contains(i.InsuranceCompanyId.ToString())).ToList();

                    if (!string.IsNullOrEmpty(insurancePolicies))
                    {
                        string[] insurancePoliciesList = insurancePolicies.Split(",");
                        insurances = insurances.Where(i => insurancePoliciesList.Contains(i.InsurancePolicyId.ToString())).ToList();
                    }
                }
            }

            var response = new List<double>
            {
                Math.Round(insurances.Sum(i => i.InsuranceAmount), 2),
                Math.Round(insurances.Sum(i => i.CancelledInsuranceAmount), 2),
                Math.Round(insurances.Sum(i => i.InsuranceBonus), 2),
                Math.Round(insurances.Sum(i => i.CancelledInsuranceBonus), 2)
            };

            return Ok(response);
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GeneralInsuranceReportResult(ReportViewModel reportViewModel)
        {
            if (ModelState.IsValid)
            {
                var insurancesGroup = await _context.Insurances
                    .Include(ic => ic.InsuranceCompany)
                    .Include(ip => ip.InsurancePolicy)
                    .Where(i => i.IsActive == !reportViewModel.IsCancelledIncluded
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

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InsurancePaymentTypeReportResult(ReportViewModel reportViewModel)
        {
            if (ModelState.IsValid)
            {
                var insurancesGroup = await _context.Insurances
                    .Include(ic => ic.InsuranceCompany)
                    .Include(ip => ip.InsurancePolicy)
                    .Where(i => i.IsActive == !reportViewModel.IsCancelledIncluded
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

            return RedirectToAction("InsurancePaymentTypeReport");
        }
    }
}