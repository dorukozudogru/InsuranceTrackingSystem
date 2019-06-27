using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SigortaTakipSistemi.Models
{
    public class CarModels
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }

        [Required]
        [ForeignKey(nameof(CarBrands))]
        public int CarBrandId { get; set; }
        [NotMapped]
        public string CarBrandName { get; set; }
        public virtual CarBrands CarBrand { get; set; }
    }
}
