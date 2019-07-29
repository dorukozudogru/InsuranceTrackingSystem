using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SigortaTakipSistemi.Models.ViewModels
{
    public class GeneralReportViewModel
    {
        [Key]
        public int Id { get; set; }

        [DisplayName("Sigorta Şirketi")]
        public string InsuranceCompanyName { get; set; }

        [DisplayName("Poliçe Tipi")]
        public string InsurancePolicyName { get; set; }

        [DisplayName("Toplam Poliçe Sayısı")]
        public int Count { get; set; }
    }
}
