using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        [DisplayName("Model Adı")]
        public string Name { get; set; }

        [Required]
        [ForeignKey(nameof(CarBrands))]
        public int CarBrandId { get; set; }
        [NotMapped]
        [DisplayName("Marka Adı")]
        public string CarBrandName { get; set; }
        public virtual CarBrands CarBrand { get; set; }

        [JsonIgnore]
        public virtual List<Insurances> Insurances { get; set; }
    }
}
