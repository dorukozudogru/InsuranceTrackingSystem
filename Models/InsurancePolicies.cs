using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SigortaTakipSistemi.Models
{
    public class InsurancePolicies
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [DisplayName("Poliçe Türü")]
        public string Name { get; set; }

        public virtual List<Insurances> Insurances { get; set; }
    }
}
