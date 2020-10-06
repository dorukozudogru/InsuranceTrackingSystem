using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SigortaTakipSistemi.Models
{
    public class InsuranceDocument
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey(nameof(Insurances))]
        public int InsuranceId { get; set; }

        [DisplayName("Doküman")]
        public byte[] Document { get; set; }

        [DisplayName("Doküman Adı")]
        public string DocumentName { get; set; }

        [DisplayName("Doküman Formatı")]
        public string DocumentFormat { get; set; }
    }
}
