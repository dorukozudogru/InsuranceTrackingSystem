using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SigortaTakipSistemi.Models
{
    public class Insurances
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Brand { get; set; }
        [Required]
        public string Model { get; set; }
        [Required]
        public string LicencePlate { get; set; }

        [Required]
        [ForeignKey(nameof(InsurancePolicy))]
        public int InsurancePolicyId { get; set; }
        [NotMapped]
        public string InsurancePolicyName { get; set; }
        public virtual InsurancePolicies InsurancePolicy { get; set; }

        [Required]
        public string InsurancePolicyNumber { get; set; }

        [Required]
        [ForeignKey(nameof(InsuranceCompany))]
        public int InsuranceCompanyId { get; set; }
        [NotMapped]
        public string InsuranceCompanyName { get; set; }
        public virtual InsuranceCompanies InsuranceCompany { get; set; }


        [Required]
        [DataType(DataType.Date)]
        public DateTime InsuranceStartDate { get; set; }
        [Required]
        [DataType(DataType.Date)]
        public DateTime InsuranceFinishDate { get; set; }

        public DateTime CreatedAt { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public int? DeletedBy { get; set; }
    }
}