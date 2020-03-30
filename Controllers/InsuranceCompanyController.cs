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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InsuranceCompanies insuranceCompany)
        {
            if (ModelState.IsValid)
            {
                _context.Add(insuranceCompany);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(insuranceCompany);
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, InsuranceCompanies insuranceCompany)
        {
            if (id != insuranceCompany.Id)
            {
                return View("Error");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var oldInsuranceCompany = await _context.InsuranceCompanies.FindAsync(id);

                    oldInsuranceCompany.Name = insuranceCompany.Name;

                    _context.Update(oldInsuranceCompany);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InsuranceCompanyExists(insuranceCompany.Id))
                    {
                        return View("Error");
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(insuranceCompany);
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var hasAnyInsurance = await _context.Insurances
                .FirstOrDefaultAsync(m => m.InsuranceCompanyId == id);

            if (hasAnyInsurance == null)
            {
                var insuranceCompany = await _context.InsuranceCompanies.FindAsync(id);
                _context.InsuranceCompanies.Remove(insuranceCompany);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            throw new TaskCanceledException("Bu şirkete ait sigorta kayıtları bulunmaktadır.");
        }

        private bool InsuranceCompanyExists(int id)
        {
            return _context.InsuranceCompanies.Any(e => e.Id == id);
        }
    }
}
