﻿using System;
using System.Linq;
using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using SigortaTakipSistemi.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace SigortaTakipSistemi.Models
{
    public class IdentityContext : IdentityDbContext<AppIdentityUser, AppIdentityRole, string>
    {
        private readonly ILoggerFactory loggerFactory;
        private readonly IHttpContextAccessor httpContextAccessor;

        public IdentityContext(DbContextOptions<IdentityContext> options, ILoggerFactory loggerFactory, IHttpContextAccessor httpContextAccessor) : base(options)
        {
            this.loggerFactory = loggerFactory;
            this.httpContextAccessor = httpContextAccessor;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLoggerFactory(loggerFactory);
        }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken))
        {
            var temoraryAuditEntities = await AuditNonTemporaryProperties();
            var result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            await AuditTemporaryProperties(temoraryAuditEntities);
            return result;
        }

        private async Task<IEnumerable<Tuple<EntityEntry, Audit>>> AuditNonTemporaryProperties()
        {
            ChangeTracker.DetectChanges();
            var entitiesToTrack = ChangeTracker.Entries().Where(e => !(e.Entity is Audit) && e.State != EntityState.Detached && e.State != EntityState.Unchanged);

            if (ChangeTracker.QueryTrackingBehavior != QueryTrackingBehavior.NoTracking)
            {
                await Audits.AddRangeAsync(
                    entitiesToTrack.Where(e => !e.Properties.Any(p => p.IsTemporary)).Select(e => new Audit()
                    {
                        TableName = e.Metadata.Relational().TableName,
                        Action = Enum.GetName(typeof(EntityState), e.State),
                        DateTime = DateTime.Now.ToUniversalTime(),
                        Username = this.httpContextAccessor?.HttpContext?.User?.Identity?.Name,
                        KeyValues = JsonConvert.SerializeObject(e.Properties.Where(p => p.Metadata.IsPrimaryKey()).ToDictionary(p => p.Metadata.Name, p => p.CurrentValue).NullIfEmpty()),
                        NewValues = JsonConvert.SerializeObject(e.Properties.Where(p => e.State == EntityState.Added || e.State == EntityState.Modified).ToDictionary(p => p.Metadata.Name, p => p.CurrentValue).NullIfEmpty()),
                        OldValues = JsonConvert.SerializeObject(e.Properties.Where(p => e.State == EntityState.Deleted || e.State == EntityState.Modified).ToDictionary(p => p.Metadata.Name, p => p.OriginalValue).NullIfEmpty())
                    }).ToList()
                );
            }
            return entitiesToTrack.Where(e => e.Properties.Any(p => p.IsTemporary))
                     .Select(e => new Tuple<EntityEntry, Audit>(
                         e,
                     new Audit()
                     {
                         TableName = e.Metadata.Relational().TableName,
                         Action = Enum.GetName(typeof(EntityState), e.State),
                         DateTime = DateTime.Now.ToUniversalTime(),
                         Username = this.httpContextAccessor?.HttpContext?.User?.Identity?.Name,
                         NewValues = JsonConvert.SerializeObject(e.Properties.Where(p => !p.Metadata.IsPrimaryKey()).ToDictionary(p => p.Metadata.Name, p => p.CurrentValue).NullIfEmpty())
                     }
                     )).ToList();
        }

        async Task AuditTemporaryProperties(IEnumerable<Tuple<EntityEntry, Audit>> temporatyEntities)
        {
            if (temporatyEntities != null && temporatyEntities.Any())
            {
                await Audits.AddRangeAsync(
                temporatyEntities.ForEach(t => t.Item2.KeyValues = JsonConvert.SerializeObject(t.Item1.Properties.Where(p => p.Metadata.IsPrimaryKey()).ToDictionary(p => p.Metadata.Name, p => p.CurrentValue).NullIfEmpty()))
                    .Select(t => t.Item2)
                );
                await SaveChangesAsync();
            }
            await Task.CompletedTask;
        }

        public DbSet<Insurances> Insurances { get; set; }

        public DbSet<InsurancePolicies> InsurancePolicies { get; set; }

        public DbSet<InsuranceCompanies> InsuranceCompanies { get; set; }

        public DbSet<CarBrands> CarBrands { get; set; }

        public DbSet<CarModels> CarModels { get; set; }

        public DbSet<Customers> Customers { get; set; }

        public DbSet<Audit> Audits { get; set; }
    }
}