using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace SigortaTakipSistemi.Models.ViewModels
{
    public class MonthlyReportViewModel
    {
        public int Month { get; set; }

        public string MonthName
        {
            get
            {
                return CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(this.Month);
            }
        }

        public int Year { get; set; }

        public int InsuranceCount { get; set; }
    }
}
