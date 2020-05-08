using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SigortaTakipSistemi.Models;

namespace SigortaTakipSistemi.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RoleController : Controller
    {
        private readonly IdentityContext _context;

        public RoleController(IdentityContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Roles.ToListAsync());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(string roleName)
        {
            AppIdentityRole appIdentityRole = new AppIdentityRole()
            {
                Name = roleName
            };

            if (ModelState.IsValid)
            {
                appIdentityRole.NormalizedName = appIdentityRole.Name;
                _context.Add(appIdentityRole);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            throw new TaskCanceledException("Rol oluşturulurken bir hata oluştu!");
        }

        //SHOULD SHOW WHO HAS ROLES
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return View("Error");
            }

            var appIdentityRole = await _context.Roles
                .FirstOrDefaultAsync(m => m.Id == id);
            if (appIdentityRole == null)
            {
                return View("Error");
            }

            return View(appIdentityRole);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return View("Error");
            }

            var appIdentityRole = await _context.Roles.FindAsync(id);
            if (appIdentityRole == null)
            {
                return View("Error");
            }
            return View(appIdentityRole);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(string id, string roleName)
        {
            var role = await _context.Roles.FindAsync(id);

            if (role != null)
            {
                role.Name = roleName;
                role.NormalizedName = roleName;
                _context.Update(role);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            throw new TaskCanceledException("Rol güncellenirken bir hata oluştu!");
        }

        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appIdentityRole = await _context.Roles
                .FirstOrDefaultAsync(m => m.Id == id);
            if (appIdentityRole == null)
            {
                return NotFound();
            }

            return View(appIdentityRole);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var hasAnyUser = await _context.UserRoles
                .FirstOrDefaultAsync(m => m.RoleId == id);

            if (hasAnyUser == null)
            {
                var appIdentityRole = await _context.Roles.FindAsync(id);
                _context.Roles.Remove(appIdentityRole);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            throw new TaskCanceledException("Bu role ait kullanıcı(lar) bulunmaktadır.");
        }

        private bool AppIdentityRoleExists(string id)
        {
            return _context.Roles.Any(e => e.Id == id);
        }
    }
}
