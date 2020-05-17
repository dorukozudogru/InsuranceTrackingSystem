using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using SigortaTakipSistemi.Models;
using static SigortaTakipSistemi.Models.Insurances;

namespace SigortaTakipSistemi.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ImportController : Controller
    {
        private readonly IdentityContext _context;

        public ImportController(IdentityContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Customer(IFormFile file, CancellationToken cancellationToken)
        {
            if (file == null || file.Length <= 0)
            {
                return BadRequest("Lütfen Bir Dosya Seçiniz!");
            }

            if (!Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Lütfen Bir EXCEL Dosyası Seçiniz!");
            }

            DateTime now = DateTime.Now;
            string loggedUser = GetLoggedUserId();

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream, cancellationToken);

                using (var package = new ExcelPackage(stream))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                    var rowCount = worksheet.Dimension.Rows;

                    for (int row = 2; row <= rowCount; row++)
                    {
                        Customers customer = new Customers()
                        {
                            Name = worksheet.Cells[row, 1].Value.ToString().Trim(),
                            Surname = worksheet.Cells[row, 2].Value.ToString().Trim(),
                            CitizenshipNo = worksheet.Cells[row, 3].Value.ToString().Trim(),
                            Email = worksheet.Cells[row, 4].Value.ToString().Trim(),
                            Phone = worksheet.Cells[row, 5].Value.ToString().Trim(),
                            Other = worksheet.Cells[row, 6].Value.ToString().Trim(),
                            IsActive = true,
                            CreatedAt = now,
                            CreatedBy = loggedUser
                        };

                        _context.Add(customer);
                        await _context.SaveChangesAsync();
                    }
                }
            }
            return RedirectToAction("Index", "Customer");
        }

        [HttpPost]
        public async Task<IActionResult> Insurance(IFormFile file, CancellationToken cancellationToken)
        {
            if (file == null || file.Length <= 0)
            {
                return BadRequest("Lütfen Bir Dosya Seçiniz!");
            }

            if (!Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Lütfen Bir EXCEL Dosyası Seçiniz!");
            }

            DateTime now = DateTime.Now;
            string loggedUser = GetLoggedUserId();

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream, cancellationToken);

                using (var package = new ExcelPackage(stream))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                    var rowCount = worksheet.Dimension.Rows;

                    for (int row = 2; row <= rowCount; row++)
                    {
                        Insurances insurance = new Insurances()
                        {
                            CustomerId = _context.Customers.FirstOrDefault(x => x.Name == worksheet.Cells[row, 1].Value.ToString().Trim() && x.Surname == worksheet.Cells[row, 2].Value.ToString().Trim()).Id,
                            CarModelId = _context.CarModels.FirstOrDefault(x => x.Name == worksheet.Cells[row, 3].Value.ToString().Trim()).Id,
                            LicencePlate = worksheet.Cells[row, 5].Value.ToString(),
                            InsurancePolicyId = _context.InsurancePolicies.FirstOrDefault(x => x.Name == worksheet.Cells[row, 6].Value.ToString().Trim()).Id,
                            InsurancePolicyNumber = worksheet.Cells[row, 7].Value.ToString().Trim(),
                            InsuranceCompanyId = _context.InsuranceCompanies.FirstOrDefault(x => x.Name == worksheet.Cells[row, 8].Value.ToString().Trim()).Id,
                            InsuranceStartDate = Convert.ToDateTime(worksheet.Cells[row, 9].Value),
                            InsuranceFinishDate = Convert.ToDateTime(worksheet.Cells[row, 10].Value),
                            InsuranceAmount = Convert.ToDouble(worksheet.Cells[row, 11].Value),
                            InsuranceBonus = Convert.ToDouble(worksheet.Cells[row, 12].Value),
                            InsuranceType = Convert.ToByte(Enum.Parse<InsuranceTypeEnum>(worksheet.Cells[row, 13].Value.ToString())),
                            InsurancePaymentType = Convert.ToByte(Enum.Parse<InsurancePaymentTypeEnum>(worksheet.Cells[row, 14].Value.ToString())),

                            InsuranceLastMailDate = DateTime.MinValue,
                            IsActive = true,
                            CreatedAt = now,
                            CreatedBy = loggedUser
                        };

                        _context.Add(insurance);
                        await _context.SaveChangesAsync();
                    }
                }
            }
            return RedirectToAction("Index", "Insurance");
        }

        public string GetLoggedUserId()
        {
            return this.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
        }

        public class DemoResponse<T>
        {
            public int Code { get; set; }

            public string Msg { get; set; }

            public T Data { get; set; }

            public static DemoResponse<T> GetResult(int code, string msg, T data = default(T))
            {
                return new DemoResponse<T>
                {
                    Code = code,
                    Msg = msg,
                    Data = data
                };
            }
        }
    }
}