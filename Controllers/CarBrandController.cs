using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SigortaTakipSistemi.Models;

namespace SigortaTakipSistemi.Controllers
{
    [Authorize]
    public class CarBrandController : Controller
    {
        private readonly IdentityContext _context;

        public CarBrandController(IdentityContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.CarBrands.OrderBy(x => x.Name).ToListAsync());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CarBrands carBrands)
        {
            if (ModelState.IsValid)
            {
                _context.Add(carBrands);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            throw new TaskCanceledException("Marka oluşturulurken bir hata oluştu!");
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return View("Error");
            }

            var carBrands = await _context.CarBrands.FindAsync(id);
            if (carBrands == null)
            {
                return View("Error");
            }
            return View(carBrands);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, string carBrandName)
        {
            var carBrand = await _context.CarBrands.FindAsync(id);

            if (carBrand != null)
            {
                carBrand.Name = carBrandName;

                _context.Update(carBrand);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            throw new TaskCanceledException("Marka güncellenirken bir hata oluştu!");
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return View("Error");
            }

            var carBrands = await _context.CarBrands
                .FirstOrDefaultAsync(m => m.Id == id);
            if (carBrands == null)
            {
                return View("Error");
            }

            return View(carBrands);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var hasAnyInsurance = await _context.Insurances
                .FirstOrDefaultAsync(m => m.CarModel.CarBrandId == id);

            if (hasAnyInsurance == null)
            {
                var carBrands = await _context.CarBrands.FindAsync(id);
                _context.CarBrands.Remove(carBrands);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            throw new TaskCanceledException("Bu markaya ait sigorta kayıtları bulunmaktadır.");
        }

        private bool CarBrandsExists(int id)
        {
            return _context.CarBrands.Any(e => e.Id == id);
        }

        public ActionResult GetCarBrands()
        {
            return Json(_context.CarBrands.OrderBy(x => x.Name).ToList());
        }
    }
}
