using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SigortaTakipSistemi.Models;

namespace SigortaTakipSistemi.Controllers
{
    [Authorize]
    public class InsuranceCompanyController : Controller
    {
        private readonly IdentityContext _context;

        public InsuranceCompanyController(IdentityContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.InsuranceCompanies.OrderBy(x => x.Name).ToListAsync());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(InsuranceCompanies insuranceCompany)
        {
            if (ModelState.IsValid)
            {
                _context.Add(insuranceCompany);
                await _context.SaveChangesAsync();
                return Ok(new { Result = true, Message = "Şirket Başarıyla Oluşturulmuştur!" });
            }
            return BadRequest("Şirket Oluşturulurken Bir Hata Oluştu!");
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return View("Error");
            }

            var insuranceCompany = await _context.InsuranceCompanies.FindAsync(id);
            if (insuranceCompany == null)
            {
                return View("Error");
            }
            return View(insuranceCompany);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, InsuranceCompanies insuranceCompanies)
        {
            var insuranceCompany = await _context.InsuranceCompanies.FindAsync(id);

            if (insuranceCompany != null)
            {
                if (ModelState.IsValid)
                {
                    insuranceCompany.Name = insuranceCompanies.Name;

                    _context.Update(insuranceCompany);
                    await _context.SaveChangesAsync();
                    return Ok(new { Result = true, Message = "Şirket Başarıyla Güncellendi!" });
                }
                else
                    return BadRequest("Tüm Alanları Doldurunuz!");
            }
            return BadRequest("Şirket Güncellenirken Bir Hata Oluştu!");
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return View("Error");
            }

            var insuranceCompany = await _context.InsuranceCompanies
                .FirstOrDefaultAsync(m => m.Id == id);
            if (insuranceCompany == null)
            {
                return View("Error");
            }

            return View(insuranceCompany);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var hasAnyInsurance = await _context.Insurances
                .FirstOrDefaultAsync(m => m.InsuranceCompanyId == id);

            if (hasAnyInsurance == null)
            {
                var insuranceCompany = await _context.InsuranceCompanies.FindAsync(id);
                _context.InsuranceCompanies.Remove(insuranceCompany);
                await _context.SaveChangesAsync();
                return Ok(new { Result = true, Message = "Şirket Silinmiştir!" });
            }
            return BadRequest("Bu Şirkete Ait Sigorta Kayıtları Bulunmaktadır!");
        }

        private bool InsuranceCompanyExists(int id)
        {
            return _context.InsuranceCompanies.Any(e => e.Id == id);
        }
    }
}
