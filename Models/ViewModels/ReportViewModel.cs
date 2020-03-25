using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SigortaTakipSistemi.Models.ViewModels
{
    public class ReportViewModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [DisplayName("Poliçe Başlama Tarihi")]
        public DateTime StartDate { get; set; }

        [Required]
        [DisplayName("Poliçe Bitiş Tarihi")]
        public DateTime FinishDate { get; set; }

        [DisplayName("Sigorta Şirketi")]
        public List<int> InsuranceCompany { get; set; }

        [DisplayName("Nakit/Kredi Kartı")]
        public List<byte> InsurancePaymentType { get; set; }

        [DisplayName("İptal Edilen Poliçeler Dahil Mi?")]
        public bool IsCancelledIncluded { get; set; }
    }
}
