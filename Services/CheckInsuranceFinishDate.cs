using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SigortaTakipSistemi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;

namespace SigortaTakipSistemi.Services
{
    internal class CheckInsuranceFinishDate : IHostedService, IDisposable
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger _logger;
        private Timer _timer;

        public CheckInsuranceFinishDate(ILogger<CheckInsuranceFinishDate> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Timed Background Service is starting.");

            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromDays(1));

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            _logger.LogInformation("Timed Background Service is working.");

            using (var scope = _scopeFactory.CreateScope())
            {
                var _context = scope.ServiceProvider.GetRequiredService<IdentityContext>();
                _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

                //var dueTimeInsurances = _context.Insurances
                //                                .Include(cu => cu.Customer)
                //                                .Include(c => c.CarModel)
                //                                .Include(cb => cb.CarModel.CarBrand)
                //                                .Include(pn => pn.InsurancePolicy)
                //                                .Include(pc => pc.InsuranceCompany)
                //                                .Where(i => i.IsActive == true && i.InsuranceFinishDate.AddDays(-8) <= DateTime.Now && i.InsuranceLastMailDate.Value.Date != DateTime.Today && i.InsuranceFinishDate >= DateTime.Now).ToList();

                var dueTimeInsurances = _context.Insurances
                                                .Include(cu => cu.Customer)
                                                .Include(c => c.CarModel)
                                                .Include(cb => cb.CarModel.CarBrand)
                                                .Include(pn => pn.InsurancePolicy)
                                                .Include(pc => pc.InsuranceCompany)
                                                .Where(i => i.IsActive == true).ToList();

                dueTimeInsurances = dueTimeInsurances.Where(x => x.InsuranceFinishDate.Date > DateTime.Now.AddDays(-8).Date && x.InsuranceFinishDate <= DateTime.Now.Date).ToList();

                dueTimeInsurances = dueTimeInsurances.Where(x => x.InsuranceLastMailDate < DateTime.Today.Date).ToList();

                var emailUsers = _context.Users.ToList();

                foreach (var insurance in dueTimeInsurances)
                {
                    SendEmail(insurance, emailUsers);
                    insurance.InsuranceLastMailDate = DateTime.Now;

                    _context.Update(insurance);
                    _context.SaveChangesAsync();
                }
            }
        }

        private void SendEmail(Insurances insurance, List<AppIdentityUser> emailUsers)
        {
            MailMessage msg = new MailMessage();
            SmtpClient smtp = new SmtpClient("smtp.gmail.com");

            smtp.Port = 587;
            smtp.Host = "smtp.gmail.com";
            smtp.EnableSsl = true;
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = null;
            smtp.Credentials = new System.Net.NetworkCredential("banazsigorta@gmail.com", "Banaz26.,");

            msg.From = new MailAddress("banazsigorta@gmail.com", "Banaz Sigorta");

            //foreach (var user in emailUsers)
            //{
            //    msg.To.Add(user.Email);
            //}

            msg.To.Add("banazsigorta@banazotomotiv.com.tr");
            
            msg.Subject = insurance.LicencePlate + " Plakalı Aracın " + insurance.InsurancePolicy.Name + " Poliçesi Süresi Dolmak Üzere";
            msg.Body = string.Format(@"Müşteri Adı: {0} - Sigorta Şirketi: {1} - Poliçe Tipi/Numarası: {2}/{3} - Poliçe Başlangıç/Bitiş Tarihi: {4} / {5}",
                               insurance.Customer.FullName,
                               insurance.InsuranceCompany.Name,
                               insurance.InsurancePolicy.Name,
                               insurance.InsurancePolicyNumber,
                               insurance.InsuranceStartDate.Date,
                               insurance.InsuranceFinishDate.Date);
            smtp.Send(msg);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Timed Background Service is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
