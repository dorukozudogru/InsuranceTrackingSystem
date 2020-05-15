using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SigortaTakipSistemi.Models;

namespace SigortaTakipSistemi.Controllers
{
    [Authorize]
    public class InsurancePolicyController : Controller
    {
        private readonly IdentityContext _context;

        public InsurancePolicyController(IdentityContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.InsurancePolicies.OrderBy(x => x.Name).ToListAsync());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(InsurancePolicies insurancePolicy)
        {
            if (ModelState.IsValid)
            {
                _context.Add(insurancePolicy);
                await _context.SaveChangesAsync();
                return Ok(new { Result = true, Message = "Poliçe Türü Başarıyla Oluşturulmuştur!" });
            }
            return BadRequest("Poliçe Türü Oluşturulurken Bir Hata Oluştu!");
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return View("Error");
            }

            var insurancePolicy = await _context.InsurancePolicies.FindAsync(id);
            if (insurancePolicy == null)
            {
                return View("Error");
            }
            return View(insurancePolicy);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, InsurancePolicies insurancePolicies)
        {
            var insurancePolicy = await _context.InsurancePolicies.FindAsync(id);

            if (insurancePolicy != null)
            {
                if (ModelState.IsValid)
                {
                    insurancePolicy.Name = insurancePolicies.Name;

                    _context.Update(insurancePolicy);
                    await _context.SaveChangesAsync();
                    return Ok(new { Result = true, Message = "Poliçe Türü Başarıyla Güncellendi!" });
                }
                else
                    return BadRequest("Tüm Alanları Doldurunuz!");
            }
            return BadRequest("Poliçe Türü Güncellenirken Bir Hata Oluştu!");
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return View("Error");
            }

            var insurancePolicy = await _context.InsurancePolicies
                .FirstOrDefaultAsync(m => m.Id == id);
            if (insurancePolicy == null)
            {
                return View("Error");
            }

            return View(insurancePolicy);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var hasAnyInsurance = await _context.Insurances
                .FirstOrDefaultAsync(m => m.InsurancePolicyId == id);

            if (hasAnyInsurance == null)
            {
                var insurancePolicy = await _context.InsurancePolicies.FindAsync(id);
                _context.InsurancePolicies.Remove(insurancePolicy);
                await _context.SaveChangesAsync();
                return Ok(new { Result = true, Message = "Poliçe Türü Silinmiştir!" });
            }
            return BadRequest("Bu Poliçe Türüne Ait Sigorta Kayıtları Bulunmaktadır!");
        }

        private bool InsurancePolicyExists(int id)
        {
            return _context.InsurancePolicies.Any(e => e.Id == id);
        }
    }
}
