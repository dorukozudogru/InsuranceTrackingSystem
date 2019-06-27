using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SigortaTakipSistemi.Models;
using System.IO;
using System.Drawing;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc.Rendering;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace SigortaTakipSistemi.Controllers
{
    public class InsuranceController : Controller
    {
        private readonly IdentityContext _context;

        public InsuranceController(IdentityContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            FakeSession.Instance.Obj = JsonConvert.SerializeObject(_context.Insurances);
            foreach (var insurance in _context.Insurances)
            {
                insurance.InsurancePolicyName = GetInsurancePoliciesById(insurance.InsurancePolicyId).Name;
                insurance.InsuranceCompanyName = GetInsuranceCompaniesById(insurance.InsuranceCompanyId).Name;
            }
            return View(await _context.Insurances.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var insurance = await _context.Insurances
                .FirstOrDefaultAsync(m => m.Id == id);
            insurance.InsurancePolicyName = GetInsurancePoliciesById(insurance.InsurancePolicyId).Name;
            insurance.InsuranceCompanyName = GetInsuranceCompaniesById(insurance.InsuranceCompanyId).Name;

            if (insurance == null)
            {
                return NotFound();
            }

            return View(insurance);
        }

        public IActionResult Create()
        {
            ViewBag.InsurancePolicies = new SelectList(GetInsurancePolicies().ToList(),"Id","Name");
            ViewBag.InsuranceCompanies = new SelectList(GetInsuranceCompanies().ToList(), "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Insurances insurance)
        {
            if (ModelState.IsValid)
            {
                _context.Add(insurance);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(insurance);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var insurance = await _context.Insurances.FindAsync(id);
            ViewBag.InsurancePolicies = new SelectList(GetInsurancePolicies().ToList(), "Id", "Name");
            ViewBag.InsuranceCompanies = new SelectList(GetInsuranceCompanies().ToList(), "Id", "Name");

            if (insurance == null)
            {
                return NotFound();
            }
            return View(insurance);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Insurances insurance)
        {
            if (id != insurance.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(insurance);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InsuranceExists(insurance.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(insurance);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var insurance = await _context.Insurances
                .FirstOrDefaultAsync(m => m.Id == id);
            insurance.InsurancePolicyName = GetInsurancePoliciesById(insurance.InsurancePolicyId).Name;
            insurance.InsuranceCompanyName = GetInsuranceCompaniesById(insurance.InsuranceCompanyId).Name;

            if (insurance == null)
            {
                return NotFound();
            }

            return View(insurance);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var insurance = await _context.Insurances.FindAsync(id);
            _context.Insurances.Remove(insurance);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool InsuranceExists(int id)
        {
            return _context.Insurances.Any(e => e.Id == id);
        }

        public async Task<IActionResult> InsuranceDateController()
        {
            //Son 20 gün kala notification göndericek.
            throw new NotImplementedException();
        }

        [Route("insurance/insurance-export")]
        public ActionResult ExportAllInsurances()
        {
            var stream = ExportInsurance(JsonConvert.DeserializeObject<List<Insurances>>(FakeSession.Instance.Obj));
            string fileName = String.Format("{0}.xlsx", "all_insurances_report");
            string fileType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            stream.Position = 0;
            return File(stream, fileType, fileName);
        }

        public MemoryStream ExportInsurance(List<Insurances> items)
        {
            var stream = new System.IO.MemoryStream();
            using (var p = new ExcelPackage(stream))
            {
                var ws = p.Workbook.Worksheets.Add("Insurances");

                using (var range = ws.Cells[1, 1, 1, 5])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(color: Color.Black);
                    range.Style.Font.Color.SetColor(Color.White);
                }

                ws.Cells[1, 1].Value = "Marka";
                ws.Cells[1, 2].Value = "Model";
                ws.Cells[1, 3].Value = "Plaka";
                ws.Cells[1, 4].Value = "Sigorta Başlama Tarihi";
                ws.Cells[1, 5].Value = "Sigorta Bitiş Tarihi";

                ws.Column(4).Style.Numberformat.Format = "dd-mmmm-yyyy";
                ws.Column(5).Style.Numberformat.Format = "dd-mmmm-yyyy";

                ws.Row(1).Style.Font.Bold = true;

                for (int c = 2; c < items.Count + 2; c++)
                {
                    ws.Cells[c, 1].Value = items[c - 2].Brand;
                    ws.Cells[c, 2].Value = items[c - 2].Model;
                    ws.Cells[c, 3].Value = items[c - 2].LicencePlate;
                    ws.Cells[c, 4].Value = items[c - 2].InsuranceStartDate;
                    ws.Cells[c, 5].Value = items[c - 2].InsuranceFinishDate;
                }
                ws.Cells[ws.Dimension.Address].AutoFitColumns();
                ws.Cells["A1:I" + items.Count + 2].AutoFilter = true;

                p.Save();
            }
            return stream;
        }

        public IOrderedQueryable<InsurancePolicies> GetInsurancePolicies()
        {
            return _context.InsurancePolicies.OrderBy(x => x.Name);
        }

        public InsurancePolicies GetInsurancePoliciesById(int Id)
        {
            return _context.InsurancePolicies.FirstOrDefault(x => x.Id == Id);
        }

        public IOrderedQueryable<InsuranceCompanies> GetInsuranceCompanies()
        {
            return _context.InsuranceCompanies.OrderBy(x => x.Name);
        }

        public InsuranceCompanies GetInsuranceCompaniesById(int Id)
        {
            return _context.InsuranceCompanies.FirstOrDefault(x => x.Id == Id);
        }

        private class FakeSession
        {
            private static FakeSession _instance;

            public static FakeSession Instance
            {
                get
                {
                    if (_instance != null)
                    {
                        return _instance;
                    }
                    _instance = new FakeSession();
                    return _instance;
                }
            }

            public string Obj { get; set; }
            public byte[] Buffer { get; set; }

        }

    }
}
