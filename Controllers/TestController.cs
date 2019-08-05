using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SigortaTakipSistemi.Helpers;
using SigortaTakipSistemi.Models;

namespace SigortaTakipSistemi.Controllers
{
    public class TestController : Controller
    {
        private readonly IdentityContext _context;

        public TestController(IdentityContext context)
        {
            _context = context;
        }

        public IActionResult ServerSidePagination()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Post()
        {
            var requestFormData = Request.Form;

            List<Insurances> insurances = await _context.Insurances
                .Include(cu => cu.Customer)
                .Include(c => c.CarModel)
                .Include(cb => cb.CarModel.CarBrand)
                .Include(pn => pn.InsurancePolicy)
                .Include(pc => pc.InsuranceCompany)
                .Where(i => i.IsActive == true)
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

        private List<Insurances> ProcessCollection(List<Insurances> lstElements, IFormCollection requestFormData)
        {
            var skip = Convert.ToInt32(requestFormData["start"].ToString());
            var pageSize = Convert.ToInt32(requestFormData["length"].ToString());
            Microsoft.Extensions.Primitives.StringValues tempOrder = new[] { "" };

            if (requestFormData.TryGetValue("order[0][column]", out tempOrder))
            {
                var columnIndex = requestFormData["order[0][column]"].ToString();
                var sortDirection = requestFormData["order[0][dir]"].ToString();
                tempOrder = new[] { "" };
                if (requestFormData.TryGetValue($"columns[{columnIndex}][data]", out tempOrder))
                {
                    var columnName = requestFormData[$"columns[{columnIndex}][data]"].ToString();
                    string searchValue = requestFormData["search[value]"].ToString().ToUpperInvariant();

                    if (pageSize > 0)
                    {
                        var prop = GetProperty(columnName);
                        if (!string.IsNullOrEmpty(searchValue))
                        {
                            if (sortDirection == "asc")
                            {
                                return lstElements.Where(l => l.InsurancePolicyNumber.Contains(searchValue)
                                || l.LicencePlate.Contains(searchValue)
                                || l.InsuranceCompany.Name.Contains(searchValue)
                                || l.Customer.FullName.Contains(searchValue)).OrderBy(prop.GetValue).Skip(skip).Take(pageSize).ToList();
                            }
                            else
                            {
                                return lstElements.Where(l => l.InsurancePolicyNumber.Contains(searchValue)
                                || l.LicencePlate.Contains(searchValue)
                                || l.InsuranceCompany.Name.Contains(searchValue)
                                || l.Customer.FullName.Contains(searchValue)).OrderByDescending(prop.GetValue).Skip(skip).Take(pageSize).ToList();
                            }
                        }
                        else
                        {
                            if (sortDirection == "asc")
                            {
                                return lstElements.OrderBy(prop.GetValue).Skip(skip).Take(pageSize).ToList();
                            }
                            else
                            {
                                return lstElements.OrderByDescending(prop.GetValue).Skip(skip).Take(pageSize).ToList();
                            }
                        }
                    }
                    else
                    {
                        return lstElements;
                    }
                }
            }
            return null;
        }

        private PropertyInfo GetProperty(string name)
        {
            var properties = typeof(Insurances).GetProperties();
            PropertyInfo prop = null;
            foreach (var item in properties)
            {
                if (item.Name.ToLowerInvariant().Equals(name.ToLowerInvariant()))
                {
                    prop = item;
                    break;
                }
            }
            return prop;
        }

        public class PaginatedResponse<T>
        {
            public List<T> Data { get; set; }

            public int Draw { get; set; }

            public int RecordsFiltered { get; set; }

            public long RecordsTotal { get; set; }
        }
    }
}