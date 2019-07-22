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

        [DisplayName("Başlangıç Tarihi")]
        public DateTime StartDate { get; set; }

        [DisplayName("Bitiş Tarihi")]
        public DateTime FinishDate { get; set; }

        [DisplayName("Sigorta Şirketi")]
        public int InsuranceCompany { get; set; }
    }
}
