using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SigortaTakipSistemi.Models;


namespace SigortaTakipSistemi.Models
{
    public class IdentityContext : IdentityDbContext<AppIdentityUser, AppIdentityRole, string>
    {
        public IdentityContext(DbContextOptions<IdentityContext> options) : base(options)
        {

        }

        public DbSet<Insurances> Insurances { get; set; }

        public DbSet<SigortaTakipSistemi.Models.InsurancePolicies> InsurancePolicies { get; set; }

        public DbSet<SigortaTakipSistemi.Models.InsuranceCompanies> InsuranceCompanies { get; set; }

        public DbSet<SigortaTakipSistemi.Models.CarBrands> CarBrands { get; set; }

        public DbSet<SigortaTakipSistemi.Models.CarModels> CarModels { get; set; }
    }
}
