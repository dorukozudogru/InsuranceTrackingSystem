using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SigortaTakipSistemi.Models;

namespace SigortaTakipSistemi.Controllers
{
    public class CarBrandController : Controller
    {
        private readonly IdentityContext _context;

        public CarBrandController(IdentityContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.CarBrands.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var carBrands = await _context.CarBrands
                .FirstOrDefaultAsync(m => m.Id == id);
            if (carBrands == null)
            {
                return NotFound();
            }

            return View(carBrands);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CarBrands carBrands)
        {
            if (ModelState.IsValid)
            {
                _context.Add(carBrands);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(carBrands);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var carBrands = await _context.CarBrands.FindAsync(id);
            if (carBrands == null)
            {
                return NotFound();
            }
            return View(carBrands);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CarBrands carBrands)
        {
            if (id != carBrands.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(carBrands);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CarBrandsExists(carBrands.Id))
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
            return View(carBrands);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var carBrands = await _context.CarBrands
                .FirstOrDefaultAsync(m => m.Id == id);
            if (carBrands == null)
            {
                return NotFound();
            }

            return View(carBrands);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var carBrands = await _context.CarBrands.FindAsync(id);
            _context.CarBrands.Remove(carBrands);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CarBrandsExists(int id)
        {
            return _context.CarBrands.Any(e => e.Id == id);
        }
    }
}
