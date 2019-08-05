using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SigortaTakipSistemi.Helpers;
using SigortaTakipSistemi.Models;
using static SigortaTakipSistemi.Helpers.ProcessCollectionHelper;

namespace SigortaTakipSistemi.Controllers
{
    [Authorize]
    public class TestController : Controller
    {
        private readonly IdentityContext _context;

        public TestController(IdentityContext context)
        {
            _context = context;
        }

        //public IActionResult ServerSidePagination()
        //{
        //    return View();
        //}

        //[HttpPost]
        //public async Task<IActionResult> Post()
        //{
        //    var requestFormData = Request.Form;

        //    List<Insurances> insurances = await _context.Insurances
        //        .Include(cu => cu.Customer)
        //        .Include(c => c.CarModel)
        //        .Include(cb => cb.CarModel.CarBrand)
        //        .Include(pn => pn.InsurancePolicy)
        //        .Include(pc => pc.InsuranceCompany)
        //        .Where(i => i.IsActive == true)
        //        .AsNoTracking()
        //        .ToListAsync();

        //    insurances = GetAllEnumNamesHelper.GetEnumName(insurances);

        //    List<Insurances> listItems = ProcessCollection(insurances, requestFormData);

        //    var response = new PaginatedResponse<Insurances>
        //    {
        //        Data = listItems,
        //        Draw = int.Parse(requestFormData["draw"]),
        //        RecordsFiltered = insurances.Count,
        //        RecordsTotal = insurances.Count
        //    };

        //    return Ok(response);
        //}
    }
}