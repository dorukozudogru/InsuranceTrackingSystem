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
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Globalization;
using SigortaTakipSistemi.Helpers;
using static SigortaTakipSistemi.Helpers.ProcessCollectionHelper;
using SigortaTakipSistemi.Models.ViewModels;

namespace SigortaTakipSistemi.Controllers
{
    public class InsuranceController : Controller
    {
        private readonly IdentityContext _context;

        public InsuranceController(IdentityContext context)
        {
            _context = context;
        }

        [Authorize]
        public IActionResult Index()
        {
            FakeSession.Instance.Obj = JsonConvert.SerializeObject(_context.Insurances.Where(i => i.IsActive == true));
            return View();
        }

        [Authorize]
        public IActionResult PassiveInsurances()
        {
            FakeSession.Instance.Obj = JsonConvert.SerializeObject(_context.Insurances.Where(i => i.IsActive == false));

            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Post(bool isActive)
        {
            var requestFormData = Request.Form;

            List<Insurances> insurances = await _context.Insurances
                .Include(cu => cu.Customer)
                .Include(c => c.CarModel)
                .Include(cb => cb.CarModel.CarBrand)
                .Include(pn => pn.InsurancePolicy)
                .Include(pc => pc.InsuranceCompany)
                .Where(i => i.IsActive == isActive)
                .AsNoTracking()
                .ToListAsync();

            insurances = GetAllEnumNamesHelper.GetEnumName(insurances);

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

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> FinishingInsurancesPost()
        {
            var requestFormData = Request.Form;

            List<Insurances> insurances = await _context.Insurances
                .Include(cu => cu.Customer)
                .Include(c => c.CarModel)
                .Include(cb => cb.CarModel.CarBrand)
                .Include(pn => pn.InsurancePolicy)
                .Include(pc => pc.InsuranceCompany)
                .Where(i => i.IsActive == true && i.InsuranceFinishDate.AddDays(-8) <= DateTime.Now && i.InsuranceFinishDate >= DateTime.Now)
                .AsNoTracking()
                .ToListAsync();

            insurances = GetAllEnumNamesHelper.GetEnumName(insurances);

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

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> MonthlyInsurances()
        {
            var insurances = await _context.Insurances
                .Where(i => i.IsActive == true &&
                       i.InsuranceStartDate.Year >= DateTime.Now.AddYears(-1).Year &&
                       i.InsuranceStartDate.Year <= DateTime.Now.Year &&
                       i.InsuranceStartDate.Month <= DateTime.Now.Month)
                .GroupBy(i => new
                {
                    Year = i.InsuranceStartDate.Year,
                    Month = i.InsuranceStartDate.Month
                })
                .Select(i => new MonthlyReportViewModel
                {
                    Year = i.Key.Year,
                    Month = i.Key.Month,
                    InsuranceCount = i.Count()
                })
                .OrderBy(i => i.Month)
                .ThenBy(i => i.Year)
                .Skip(2)
                .ToListAsync();

            return Json(insurances);
        }

        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return View("Error");
            }

            var insurance = await _context.Insurances
                .Include(cu => cu.Customer)
                .Include(c => c.CarModel)
                .Include(cb => cb.CarModel.CarBrand)
                .Include(pn => pn.InsurancePolicy)
                .Include(pc => pc.InsuranceCompany)
                .FirstOrDefaultAsync(m => m.Id == id);

            #region InsuranceType
            if (insurance.InsuranceType == 0)
            {
                insurance.InsuranceTypeName = "SIFIR";
            }
            else if (insurance.InsuranceType == 1)
            {
                insurance.InsuranceTypeName = "YENİLEME";
            }
            else if (insurance.InsuranceType == 2)
            {
                insurance.InsuranceTypeName = "2. EL";
            }
            #endregion

            #region InsurancePaymentType
            if (insurance.InsurancePaymentType == 0)
            {
                insurance.InsurancePaymentTypeName = "NAKİT";
            }
            else if (insurance.InsurancePaymentType == 1)
            {
                insurance.InsurancePaymentTypeName = "KREDİ KARTI";
            }
            else if (insurance.InsurancePaymentType == 2)
            {
                insurance.InsurancePaymentTypeName = "BEDELSİZ";
            }
            #endregion

            if (insurance == null)
            {
                return View("Error");
            }

            return View(insurance);
        }

        [Authorize]
        public IActionResult Create()
        {
            ViewBag.InsurancePolicies = new SelectList(_context.InsurancePolicies.OrderBy(x => x.Name), "Id", "Name");
            ViewBag.InsuranceCompanies = new SelectList(_context.InsuranceCompanies.OrderBy(x => x.Name), "Id", "Name");
            ViewBag.Customers = new SelectList(_context.Customers.Where(x => x.IsActive == true).OrderBy(x => x.Name), "Id", "FullName");
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Insurances insurance)
        {
            if (ModelState.IsValid)
            {
                //insurance.InsuranceBonus = InsuranceBonusCalculation(insurance.InsuranceAmount, _context.InsurancePolicies.FirstOrDefault(pn => pn.Id == insurance.InsurancePolicyId).Name);
                insurance.CreatedBy = GetLoggedUserId();
                insurance.CreatedAt = DateTime.Now;
                insurance.IsActive = true;
                insurance.InsuranceLastMailDate = DateTime.MinValue;

                insurance.CarModelId = _context.CarModels.FirstOrDefault(x => x.Name == insurance.CarModel.Name).Id;
                insurance.CarModel.CarBrandId = _context.CarBrands.FirstOrDefault(x => x.Name == insurance.CarModel.CarBrand.Name).Id;

                _context.Add(insurance);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(insurance);
        }

        [Authorize]
        public IActionResult Edit()
        {
            ViewBag.InsurancePolicies = new SelectList(_context.InsurancePolicies.OrderBy(x => x.Name), "Id", "Name");
            ViewBag.InsuranceCompanies = new SelectList(_context.InsuranceCompanies.OrderBy(x => x.Name), "Id", "Name");
            ViewBag.Customers = new SelectList(_context.Customers.Where(x => x.IsActive == true).OrderBy(x => x.Name), "Id", "FullName");
            ViewBag.CarBrands = new SelectList(_context.CarBrands.OrderBy(x => x.Name), "Id", "Name");
            ViewBag.CarModels = new SelectList(_context.CarModels.OrderBy(x => x.Name), "Name", "Name");

            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> EditPost(int? id)
        {
            if (id == null)
            {
                return View("Error");
            }

            var insurance = await _context.Insurances
                .Include(cu => cu.Customer)
                .Include(c => c.CarModel)
                .Include(cb => cb.CarModel.CarBrand)
                .Include(pn => pn.InsurancePolicy)
                .Include(pc => pc.InsuranceCompany)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (insurance == null)
            {
                return View("Error");
            }

            return Ok(insurance);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Edit(int id, Insurances insurance)
        {
            var oldInsurance = await _context.Insurances
                   .Include(cu => cu.Customer)
                   .Include(c => c.CarModel)
                   .Include(cb => cb.CarModel.CarBrand)
                   .Include(pn => pn.InsurancePolicy)
                   .Include(pc => pc.InsuranceCompany)
                   .FirstOrDefaultAsync(m => m.Id == id);

            if (oldInsurance != null)
            {
                if (ModelState.IsValid)
                {
                    oldInsurance.CarModelId = _context.CarModels.FirstOrDefault(x => x.Name == insurance.CarModel.Name).Id;
                    oldInsurance.CustomerId = insurance.CustomerId;
                    oldInsurance.InsuranceAmount = insurance.InsuranceAmount;
                    oldInsurance.InsuranceBonus = insurance.InsuranceBonus;
                    oldInsurance.InsuranceCompanyId = insurance.InsuranceCompanyId;
                    oldInsurance.InsuranceFinishDate = insurance.InsuranceFinishDate;
                    oldInsurance.InsurancePaymentType = insurance.InsurancePaymentType;
                    oldInsurance.InsurancePolicyId = insurance.InsurancePolicyId;
                    oldInsurance.InsurancePolicyNumber = insurance.InsurancePolicyNumber;
                    oldInsurance.InsuranceStartDate = insurance.InsuranceStartDate;
                    oldInsurance.InsuranceType = insurance.InsuranceType;
                    oldInsurance.LicencePlate = insurance.LicencePlate;
                    oldInsurance.UpdatedAt = DateTime.Now;
                    oldInsurance.UpdatedBy = GetLoggedUserId();

                    _context.Update(oldInsurance);
                    await _context.SaveChangesAsync();
                    return Ok(new { Result = true, Message = "Poliçe Başarıyla Güncellendi!" });
                }
                else
                    return BadRequest("Tüm Alanları Doldurunuz!");
            }
            return BadRequest("Poliçe Güncellenirken Bir Hata Oluştu!");
        }

        [Authorize]
        public async Task<IActionResult> Passive(int? id)
        {
            if (id == null)
            {
                return View("Error");
            }

            var insurance = await _context.Insurances
                .Include(cu => cu.Customer)
                .Include(c => c.CarModel)
                .Include(cb => cb.CarModel.CarBrand)
                .Include(pn => pn.InsurancePolicy)
                .Include(pc => pc.InsuranceCompany)
                .FirstOrDefaultAsync(m => m.Id == id);

            #region InsuranceType
            if (insurance.InsuranceType == 0)
            {
                insurance.InsuranceTypeName = "SIFIR";
            }
            else if (insurance.InsuranceType == 1)
            {
                insurance.InsuranceTypeName = "YENİLEME";
            }
            else if (insurance.InsuranceType == 2)
            {
                insurance.InsuranceTypeName = "2. EL";
            }
            #endregion

            #region InsurancePaymentType
            if (insurance.InsurancePaymentType == 0)
            {
                insurance.InsurancePaymentTypeName = "NAKİT";
            }
            else if (insurance.InsurancePaymentType == 1)
            {
                insurance.InsurancePaymentTypeName = "KREDİ KARTI";
            }
            else if (insurance.InsurancePaymentType == 2)
            {
                insurance.InsurancePaymentTypeName = "BEDELSİZ";
            }
            #endregion

            if (insurance == null)
            {
                return View("Error");
            }

            return View(insurance);
        }

        [Authorize]
        [HttpPost, ActionName("Passive")]
        public async Task<IActionResult> PassiveConfirmed(int id)
        {
            var insurance = await _context.Insurances.FindAsync(id);

            if (insurance != null)
            {
                insurance.IsActive = false;
                insurance.DeletedAt = DateTime.Now;
                insurance.DeletedBy = GetLoggedUserId();

                _context.Update(insurance);
                await _context.SaveChangesAsync();
                return Ok(new { Result = true, Message = "Poliçe Pasif Olarak Ayarlanmıştır!" });
            }
            return BadRequest("Poliçe Pasif Olarak Ayarlanırken Bir Hata Oluştu!");
        }

        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return View("Error");
            }

            var insurance = await _context.Insurances
                .Include(cu => cu.Customer)
                .Include(c => c.CarModel)
                .Include(cb => cb.CarModel.CarBrand)
                .Include(pn => pn.InsurancePolicy)
                .Include(pc => pc.InsuranceCompany)
                .FirstOrDefaultAsync(m => m.Id == id);

            #region InsuranceType
            if (insurance.InsuranceType == 0)
            {
                insurance.InsuranceTypeName = "SIFIR";
            }
            else if (insurance.InsuranceType == 1)
            {
                insurance.InsuranceTypeName = "YENİLEME";
            }
            else if (insurance.InsuranceType == 2)
            {
                insurance.InsuranceTypeName = "2. EL";
            }
            #endregion

            #region InsurancePaymentType
            if (insurance.InsurancePaymentType == 0)
            {
                insurance.InsurancePaymentTypeName = "NAKİT";
            }
            else if (insurance.InsurancePaymentType == 1)
            {
                insurance.InsurancePaymentTypeName = "KREDİ KARTI";
            }
            else if (insurance.InsurancePaymentType == 2)
            {
                insurance.InsurancePaymentTypeName = "BEDELSİZ";
            }
            #endregion

            if (insurance == null)
            {
                return View("Error");
            }

            return View(insurance);
        }

        [Authorize]
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var insurance = await _context.Insurances.FindAsync(id);

            if (insurance != null)
            {
                _context.Insurances.Remove(insurance);

                await _context.SaveChangesAsync();
                return Ok(new { Result = true, Message = "Poliçe Silinmiştir!" });
            }
            return BadRequest("Poliçe Silinirken Bir Hata Oluştu!");
        }

        [Authorize]
        public async Task<IActionResult> Revoke(int? id)
        {
            if (id == null)
            {
                return View("Error");
            }

            var insurance = await _context.Insurances
                .Include(cu => cu.Customer)
                .Include(c => c.CarModel)
                .Include(cb => cb.CarModel.CarBrand)
                .Include(pn => pn.InsurancePolicy)
                .Include(pc => pc.InsuranceCompany)
                .FirstOrDefaultAsync(m => m.Id == id);

            insurance.InsurancePolicyName = GetInsurancePoliciesById(insurance.InsurancePolicyId).Name;
            insurance.InsuranceCompanyName = GetInsuranceCompaniesById(insurance.InsuranceCompanyId).Name;

            #region InsuranceType
            if (insurance.InsuranceType == 0)
            {
                insurance.InsuranceTypeName = "SIFIR";
            }
            else if (insurance.InsuranceType == 1)
            {
                insurance.InsuranceTypeName = "YENİLEME";
            }
            else if (insurance.InsuranceType == 2)
            {
                insurance.InsuranceTypeName = "2. EL";
            }
            #endregion

            #region InsurancePaymentType
            if (insurance.InsurancePaymentType == 0)
            {
                insurance.InsurancePaymentTypeName = "NAKİT";
            }
            else if (insurance.InsurancePaymentType == 1)
            {
                insurance.InsurancePaymentTypeName = "KREDİ KARTI";
            }
            else if (insurance.InsurancePaymentType == 2)
            {
                insurance.InsurancePaymentTypeName = "BEDELSİZ";
            }
            #endregion

            if (insurance == null)
            {
                return View("Error");
            }

            return View(insurance);
        }

        [Authorize]
        [HttpPost, ActionName("Revoke")]
        public async Task<IActionResult> RevokeConfirmed(int id)
        {
            var insurance = await _context.Insurances.FindAsync(id);

            if (insurance != null)
            {
                insurance.IsActive = true;
                insurance.UpdatedAt = DateTime.Now;
                insurance.UpdatedBy = GetLoggedUserId();

                insurance.DeletedAt = null;
                insurance.DeletedBy = null;

                _context.Update(insurance);
                await _context.SaveChangesAsync();
                return Ok(new { Result = true, Message = "Poliçe Aktif Olarak Ayarlanmıştır!" });
            }
            return BadRequest("Poliçe Aktif Olarak Ayarlanırken Bir Hata Oluştu!");
        }

        [Authorize]
        public async Task<IActionResult> Cancel(int? id)
        {
            if (id == null)
            {
                return View("Error");
            }

            var insurance = await _context.Insurances
                .Include(cu => cu.Customer)
                .Include(c => c.CarModel)
                .Include(cb => cb.CarModel.CarBrand)
                .Include(pn => pn.InsurancePolicy)
                .Include(pc => pc.InsuranceCompany)
                .FirstOrDefaultAsync(m => m.Id == id);

            #region InsuranceType
            if (insurance.InsuranceType == 0)
            {
                insurance.InsuranceTypeName = "SIFIR";
            }
            else if (insurance.InsuranceType == 1)
            {
                insurance.InsuranceTypeName = "YENİLEME";
            }
            else if (insurance.InsuranceType == 2)
            {
                insurance.InsuranceTypeName = "2. EL";
            }
            #endregion

            #region InsurancePaymentType
            if (insurance.InsurancePaymentType == 0)
            {
                insurance.InsurancePaymentTypeName = "NAKİT";
            }
            else if (insurance.InsurancePaymentType == 1)
            {
                insurance.InsurancePaymentTypeName = "KREDİ KARTI";
            }
            else if (insurance.InsurancePaymentType == 2)
            {
                insurance.InsurancePaymentTypeName = "BEDELSİZ";
            }
            #endregion

            if (insurance == null)
            {
                return View("Error");
            }

            return View(insurance);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Cancel(int id, double cancelledInsuranceAmount, double cancelledInsuranceBonus)
        {
            var insurance = await _context.Insurances.FindAsync(id);

            if (insurance != null)
            {
                if (insurance.InsuranceAmount >= cancelledInsuranceAmount)
                {
                    if (ModelState.IsValid)
                    {
                        insurance.IsActive = false;
                        insurance.CancelledAt = DateTime.Now;
                        insurance.CancelledInsuranceAmount = cancelledInsuranceAmount;
                        insurance.CancelledInsuranceBonus = cancelledInsuranceBonus;

                        insurance.DeletedAt = DateTime.Now;
                        insurance.DeletedBy = GetLoggedUserId();

                        _context.Update(insurance);
                        await _context.SaveChangesAsync();
                        return Ok(new { Result = true, Message = "Poliçe İptal Edilmiştir!" });
                    }
                }
                else
                    return BadRequest("İptal Poliçe Tutarı Normal Tutardan Fazla Olamaz!");
            }
            return BadRequest("Poliçe İptal Edilirken Bir Hata Oluştu!");
        }

        [Authorize]
        public bool InsuranceExists(int id)
        {
            return _context.Insurances.Any(e => e.Id == id);
        }

        [Authorize(Roles = "Admin")]
        [Route("insurance/insurance-export")]
        public ActionResult ExportAllInsurances()
        {
            var pageName = Request.Headers["Referer"].ToString()?.Split("/");
            var stream = ExportInsurance(JsonConvert.DeserializeObject<List<Insurances>>(FakeSession.Instance.Obj), 1, pageName.Last());
            string fileName = String.Format("{0}.xlsx", "all_insurances_report");
            string fileType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            stream.Position = 0;
            return File(stream, fileType, fileName);
        }

        [Authorize(Roles = "Admin")]
        [Route("insurance/insurance-active-export")]
        public ActionResult ExportAllActiveInsurances()
        {
            var stream = ExportInsurance(_context.Insurances
                .Include(cu => cu.Customer)
                .Include(c => c.CarModel)
                .Include(cb => cb.CarModel.CarBrand)
                .Include(pn => pn.InsurancePolicy)
                .Include(pc => pc.InsuranceCompany)
                .Where(i => i.IsActive == true).ToList(), 0, "ActiveInsurances");
            string fileName = String.Format("{0}.xlsx", "all_active_insurances_report");
            string fileType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            stream.Position = 0;
            return File(stream, fileType, fileName);
        }

        [Authorize(Roles = "Admin")]
        [Route("insurance/insurance-passive-export")]
        public ActionResult ExportAllPassiveInsurances()
        {
            var stream = ExportInsurance(_context.Insurances
                .Include(cu => cu.Customer)
                .Include(c => c.CarModel)
                .Include(cb => cb.CarModel.CarBrand)
                .Include(pn => pn.InsurancePolicy)
                .Include(pc => pc.InsuranceCompany)
                .Where(i => i.IsActive == false).ToList(), 0, "PassiveInsurances");
            string fileName = String.Format("{0}.xlsx", "all_passive_insurances_report");
            string fileType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            stream.Position = 0;
            return File(stream, fileType, fileName);
        }

        [Authorize(Roles = "Admin")]
        public MemoryStream ExportInsurance(List<Insurances> items, byte type, string pageName)
        {
            var stream = new System.IO.MemoryStream();

            using (var p = new ExcelPackage(stream))
            {
                var ws = p.Workbook.Worksheets.Add("Poliçeler");

                using (var range = ws.Cells[1, 1, 1, 19])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(color: Color.Black);
                    range.Style.Font.Color.SetColor(Color.White);
                }

                ws.Cells[1, 1].Value = "ID";
                ws.Cells[1, 2].Value = "Poliçe Tipi";
                ws.Cells[1, 3].Value = "Poliçe Numarası";
                ws.Cells[1, 4].Value = "Sigorta Firması";
                ws.Cells[1, 5].Value = "Marka";
                ws.Cells[1, 6].Value = "Model";
                ws.Cells[1, 7].Value = "Plaka";
                ws.Cells[1, 8].Value = "Müşteri Adı Soyadı";
                ws.Cells[1, 9].Value = "Müşteri Telefonu";
                ws.Cells[1, 10].Value = "Müşteri E-Postası";
                ws.Cells[1, 11].Value = "Poliçe Başlama Tarihi";
                ws.Cells[1, 12].Value = "Poliçe Bitiş Tarihi";
                ws.Cells[1, 13].Value = "İptal Edilme Tarihi";
                ws.Cells[1, 14].Value = "Poliçe Tutarı";
                ws.Cells[1, 15].Value = "İptal Edilen Poliçe Tutarı";
                ws.Cells[1, 16].Value = "Poliçe Primi";
                ws.Cells[1, 17].Value = "İptal İşleminden Sonra Prim Tutarı";
                ws.Cells[1, 18].Value = "Sıfır/Yenileme";
                ws.Cells[1, 19].Value = "Nakit/Kredi Kartı";

                ws.Column(11).Style.Numberformat.Format = "dd-mmmm-yyyy";
                ws.Column(12).Style.Numberformat.Format = "dd-mmmm-yyyy";
                ws.Column(13).Style.Numberformat.Format = "dd-mmmm-yyyy";

                ws.Row(1).Style.Font.Bold = true;

                for (int c = 2; c < items.Count + 2; c++)
                {
                    ws.Cells[c, 1].Value = items[c - 2].Id;
                    ws.Cells[c, 2].Value = items[c - 2].InsurancePolicy.Name;
                    ws.Cells[c, 3].Value = items[c - 2].InsurancePolicyNumber;
                    ws.Cells[c, 4].Value = items[c - 2].InsuranceCompany.Name;
                    ws.Cells[c, 5].Value = items[c - 2].CarModel.CarBrand.Name;
                    ws.Cells[c, 6].Value = items[c - 2].CarModel.Name;
                    ws.Cells[c, 7].Value = items[c - 2].LicencePlate;
                    ws.Cells[c, 8].Value = items[c - 2].Customer.FullName;
                    ws.Cells[c, 9].Value = items[c - 2].Customer.Phone;
                    ws.Cells[c, 10].Value = items[c - 2].Customer.Email;
                    ws.Cells[c, 11].Value = items[c - 2].InsuranceStartDate;
                    ws.Cells[c, 12].Value = items[c - 2].InsuranceFinishDate;
                    ws.Cells[c, 13].Value = items[c - 2].CancelledAt;
                    ws.Cells[c, 14].Value = items[c - 2].InsuranceAmount;
                    ws.Cells[c, 15].Value = items[c - 2].CancelledInsuranceAmount;
                    ws.Cells[c, 16].Value = items[c - 2].InsuranceBonus;
                    ws.Cells[c, 17].Value = items[c - 2].CancelledInsuranceBonus;

                    #region InsuranceType
                    if (items[c - 2].InsuranceType == 0)
                    {
                        ws.Cells[c, 18].Value = "SIFIR";
                    }
                    else if (items[c - 2].InsuranceType == 1)
                    {
                        ws.Cells[c, 18].Value = "YENİLEME";
                    }
                    else if (items[c - 2].InsuranceType == 2)
                    {
                        ws.Cells[c, 18].Value = "2. EL";
                    }
                    #endregion

                    #region InsurancePaymentType
                    if (items[c - 2].InsurancePaymentType == 0)
                    {
                        ws.Cells[c, 19].Value = "NAKİT";
                    }
                    else if (items[c - 2].InsurancePaymentType == 1)
                    {
                        ws.Cells[c, 19].Value = "KREDİ KARTI";
                    }
                    else if (items[c - 2].InsurancePaymentType == 2)
                    {
                        ws.Cells[c, 19].Value = "BEDELSİZ";
                    }
                    #endregion
                }

                var lastRow = ws.Dimension.End.Row;
                var lastColumn = ws.Dimension.End.Column;

                if (type == 1)
                {
                    using (var range = ws.Cells[lastRow + 1, 1, lastRow + 1, lastColumn])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(color: Color.Gray);
                        range.Style.Font.Color.SetColor(Color.White);
                    }

                    ws.Cells[lastRow + 1, 13].Value = "Toplam:";
                    ws.Cells[lastRow + 1, 14].Formula = String.Format("SUM(N2:N{0})", lastRow);
                    ws.Cells[lastRow + 1, 15].Formula = String.Format("SUM(O2:O{0})", lastRow);
                    ws.Cells[lastRow + 1, 16].Formula = String.Format("SUM(P2:P{0})", lastRow);
                    ws.Cells[lastRow + 1, 17].Formula = String.Format("SUM(Q2:Q{0})", lastRow);
                }

                ws.Cells[ws.Dimension.Address].AutoFitColumns();
                ws.Cells["A1:S" + items.Count + 2].AutoFilter = true;

                ws.Column(19).PageBreak = true;
                ws.PrinterSettings.PaperSize = ePaperSize.A4;
                ws.PrinterSettings.Orientation = eOrientation.Landscape;
                ws.PrinterSettings.Scale = 50;

                p.Save();
            }
            AddExportAudit(pageName);
            return stream;
        }

        [Authorize]
        public InsurancePolicies GetInsurancePoliciesById(int Id)
        {
            return _context.InsurancePolicies.FirstOrDefault(x => x.Id == Id);
        }

        [Authorize]
        public InsuranceCompanies GetInsuranceCompaniesById(int Id)
        {
            return _context.InsuranceCompanies.FirstOrDefault(x => x.Id == Id);
        }

        [Authorize]
        public string GetLoggedUserId()
        {
            return this.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
        }

        [Authorize]
        public double InsuranceBonusCalculation(double amount, string insurancePolicy)
        {
            double bonus = 0.0;

            if (insurancePolicy.ToUpper(new CultureInfo("tr-TR")) == "DASK")
            {
                bonus = (amount * 25) / 100;
            }
            else if (insurancePolicy.ToUpper(new CultureInfo("tr-TR")) == "TRAFİK")
            {
                bonus = (amount * 10) / 100;
            }
            else if (insurancePolicy.ToUpper(new CultureInfo("tr-TR")) == "KASKO")
            {
                bonus = (amount * 14.5) / 100;
            }
            else if (insurancePolicy.ToUpper(new CultureInfo("tr-TR")) == "İŞYERİ")
            {
                bonus = (amount * 25) / 100;
            }
            else if (insurancePolicy.ToUpper(new CultureInfo("tr-TR")) == "TEHLİKELİ MADDELER")
            {
                bonus = (amount * 10) / 100;
            }
            return bonus;
        }

        [Authorize]
        public void AddExportAudit(string pageName)
        {
            Audit audit = new Audit()
            {
                Action = "Exported",
                DateTime = DateTime.Now.ToUniversalTime(),
                KeyValues = "{\"Id\":\"-\"}",
                NewValues = "{\"PageName\":\"" + pageName + "\"}",
                TableName = pageName,
                Username = HttpContext?.User?.Identity?.Name
            };
            _context.Add(audit);
            _context.SaveChanges();
        }

        public class FakeSession
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
