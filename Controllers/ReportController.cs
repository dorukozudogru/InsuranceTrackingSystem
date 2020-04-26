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

            List<Insurances> insurances = new List<Insurances>();

            var cancelledInsurances = GetCancelledInsurances().Result;
            var activeInsurances = GetActiveInsurances().Result;
            var passiveInsurances = GetPassiveInsurances().Result;

            cancelledInsurances = cancelledInsurances
                .Where(i => i.InsuranceStartDate >= startDate
                          && i.InsuranceStartDate <= finishDate)
                .ToList();

            activeInsurances = activeInsurances
                .Where(i => i.InsuranceStartDate >= startDate
                          && i.InsuranceStartDate <= finishDate)
                .ToList();

            passiveInsurances = passiveInsurances
                .Where(i => i.InsuranceStartDate >= startDate
                          && i.InsuranceStartDate <= finishDate)
                .ToList();

            if (!isCancelledIncluded)
            {
                insurances = activeInsurances;
            }
            else
            {
                insurances = activeInsurances.Concat(cancelledInsurances).ToList();
            }

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
            List<Insurances> insurances = new List<Insurances>();

            var cancelledInsurances = GetCancelledInsurances().Result;
            var activeInsurances = GetActiveInsurances().Result;
            var passiveInsurances = GetPassiveInsurances().Result;

            cancelledInsurances = cancelledInsurances
                .Where(i => i.InsuranceStartDate >= startDate
                          && i.InsuranceStartDate <= finishDate)
                .ToList();

            activeInsurances = activeInsurances
                .Where(i => i.InsuranceStartDate >= startDate
                          && i.InsuranceStartDate <= finishDate)
                .ToList();

            passiveInsurances = passiveInsurances
                .Where(i => i.InsuranceStartDate >= startDate
                          && i.InsuranceStartDate <= finishDate)
                .ToList();

            if (!isCancelledIncluded)
            {
                insurances = activeInsurances;
            }
            else
            {
                insurances = activeInsurances.Concat(cancelledInsurances).ToList();
            }

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
                List<GeneralReportViewModel> insurancesGroup = new List<GeneralReportViewModel>();
                List<Insurances> insurances = new List<Insurances>();

                var cancelledInsurances = GetCancelledInsurances().Result;
                var activeInsurances = GetActiveInsurances().Result;
                var passiveInsurances = GetPassiveInsurances().Result;

                cancelledInsurances = cancelledInsurances
                    .Where(i => i.InsuranceStartDate >= reportViewModel.StartDate
                              && i.InsuranceStartDate <= reportViewModel.FinishDate)
                    .ToList();

                activeInsurances = activeInsurances
                    .Where(i => i.InsuranceStartDate >= reportViewModel.StartDate
                              && i.InsuranceStartDate <= reportViewModel.FinishDate)
                    .ToList();

                passiveInsurances = passiveInsurances
                    .Where(i => i.InsuranceStartDate >= reportViewModel.StartDate
                              && i.InsuranceStartDate <= reportViewModel.FinishDate)
                    .ToList();

                if (!reportViewModel.IsCancelledIncluded)
                {
                    insurancesGroup = activeInsurances
                                         .Where(i => i.InsuranceStartDate >= reportViewModel.StartDate
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
                                         .ToList();
                }
                else
                {
                    insurances = activeInsurances.Concat(cancelledInsurances).ToList();

                    insurancesGroup = insurances
                                         .Where(i => i.InsuranceStartDate >= reportViewModel.StartDate
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
                                         .ToList();
                }
                return View(insurancesGroup);
            }
            return RedirectToAction("GeneralInsuranceReport");
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InsurancePaymentTypeReportResult(ReportViewModel reportViewModel)
        {
            if (ModelState.IsValid)
            {
                List<GeneralReportViewModel> insurancesGroup = new List<GeneralReportViewModel>();
                List<Insurances> insurances = new List<Insurances>();

                var cancelledInsurances = GetCancelledInsurances().Result;
                var activeInsurances = GetActiveInsurances().Result;
                var passiveInsurances = GetPassiveInsurances().Result;

                cancelledInsurances = cancelledInsurances
                    .Where(i => i.InsuranceStartDate >= reportViewModel.StartDate
                              && i.InsuranceStartDate <= reportViewModel.FinishDate)
                    .ToList();

                activeInsurances = activeInsurances
                    .Where(i => i.InsuranceStartDate >= reportViewModel.StartDate
                              && i.InsuranceStartDate <= reportViewModel.FinishDate)
                    .ToList();

                passiveInsurances = passiveInsurances
                    .Where(i => i.InsuranceStartDate >= reportViewModel.StartDate
                              && i.InsuranceStartDate <= reportViewModel.FinishDate)
                    .ToList();

                if (!reportViewModel.IsCancelledIncluded)
                {
                    insurancesGroup = activeInsurances
                                         .Where(i => i.InsuranceStartDate >= reportViewModel.StartDate
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
                                              .ToList();
                }
                else
                {
                    insurances = activeInsurances.Concat(cancelledInsurances).ToList();

                    insurancesGroup = insurances
                                         .Where(i => i.InsuranceStartDate >= reportViewModel.StartDate
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
                                              .ToList();
                }

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

        public async Task<List<Insurances>> GetActiveInsurances()
        {
            return await _context.Insurances
                .Include(cu => cu.Customer)
                .Include(c => c.CarModel)
                .Include(cb => cb.CarModel.CarBrand)
                .Include(pn => pn.InsurancePolicy)
                .Include(pc => pc.InsuranceCompany)
                .Where(i => i.IsActive == true)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<Insurances>> GetPassiveInsurances()
        {
            return await _context.Insurances
                .Include(cu => cu.Customer)
                .Include(c => c.CarModel)
                .Include(cb => cb.CarModel.CarBrand)
                .Include(pn => pn.InsurancePolicy)
                .Include(pc => pc.InsuranceCompany)
                .Where(i => i.IsActive == false && (i.CancelledAt == null && i.CancelledInsuranceAmount == 0 && i.CancelledInsuranceBonus == 0))
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<Insurances>> GetCancelledInsurances()
        {
            return await _context.Insurances
                .Include(cu => cu.Customer)
                .Include(c => c.CarModel)
                .Include(cb => cb.CarModel.CarBrand)
                .Include(pn => pn.InsurancePolicy)
                .Include(pc => pc.InsuranceCompany)
                .Where(i => i.IsActive == false && (i.CancelledAt != null && i.CancelledInsuranceAmount != 0 && i.CancelledInsuranceBonus != 0))
                .AsNoTracking()
                .ToListAsync();
        }
    }
}