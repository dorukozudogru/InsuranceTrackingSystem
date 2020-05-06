using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SigortaTakipSistemi.Models;

namespace SigortaTakipSistemi.Controllers
{
    [Authorize]
    public class CarModelController : Controller
    {
        private readonly IdentityContext _context;

        public CarModelController(IdentityContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var identityContext = _context.CarModels.Include(c => c.CarBrand);
            return View(await identityContext.OrderBy(x => x.CarBrandId).ToListAsync());
        }

        public IActionResult Create()
        {
            ViewData["CarBrandId"] = new SelectList(_context.CarBrands, "Id", "Name");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CarModels carModels)
        {
            if (ModelState.IsValid)
            {
                _context.Add(carModels);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            throw new TaskCanceledException("Model oluşturulurken bir hata oluştu!");
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return View("Error");
            }

            var carModels = await _context.CarModels.FindAsync(id);
            if (carModels == null)
            {
                return View("Error");
            }
            ViewData["CarBrandId"] = new SelectList(_context.CarBrands, "Id", "Name", carModels.CarBrandId);
            return View(carModels);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, string carModelName, int carBrandId)
        {
            var carModel = await _context.CarModels.FindAsync(id);

            if (carModel != null)
            {
                carModel.Name = carModelName;
                carModel.CarBrandId = carBrandId;

                _context.Update(carModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            throw new TaskCanceledException("Model güncellenirken bir hata oluştu!");
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return View("Error");
            }

            var carModels = await _context.CarModels
                .Include(c => c.CarBrand)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (carModels == null)
            {
                return View("Error");
            }

            return View(carModels);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var hasAnyInsurance = await _context.Insurances
                .FirstOrDefaultAsync(m => m.CarModelId == id);

            if (hasAnyInsurance == null)
            {
                var carModels = await _context.CarModels.FindAsync(id);
                _context.Remove(carModels);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            throw new TaskCanceledException("Bu modele ait sigorta kayıtları bulunmaktadır.");
        }

        private bool CarModelsExists(int id)
        {
            return _context.CarModels.Any(e => e.Id == id);
        }

        public ActionResult GetCarModelsById(int Id)
        {
            return Json(_context.CarModels.Where(x => x.CarBrandId == Id).OrderBy(x => x.Name).ToList());
        }
    }
}
