using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SigortaTakipSistemi.Models.ViewModels;

namespace SigortaTakipSistemi.Models
{
    public class IdentityContext : IdentityDbContext<AppIdentityUser, AppIdentityRole, string>
    {
        public IdentityContext(DbContextOptions<IdentityContext> options) : base(options)
        {

        }

        public DbSet<Insurances> Insurances { get; set; }

        public DbSet<InsurancePolicies> InsurancePolicies { get; set; }

        public DbSet<InsuranceCompanies> InsuranceCompanies { get; set; }

        public DbSet<CarBrands> CarBrands { get; set; }

        public DbSet<CarModels> CarModels { get; set; }

        public DbSet<Customers> Customers { get; set; }

        public DbSet<SigortaTakipSistemi.Models.ViewModels.ReportViewModel> ReportViewModel { get; set; }
    }
}
