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
    public class Customers
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [DisplayName("Müşteri Adı")]
        public string Name { get; set; }
        [Required]
        [DisplayName("Müşteri Soyadı")]
        public string Surname { get; set; }

        [NotMapped]
        private string _fullName { get; set; }
        [NotMapped]
        [DisplayName("Müşteri Adı Soyadı")]
        public string FullName
        {
            get
            {
                if (string.IsNullOrEmpty(_fullName))
                {
                    return string.Format($"{this.Name} {this.Surname}");
                }
                return _fullName;
            }
            set
            {
                _fullName = value;
            }
        }

        [Required]
        [DisplayName("Kimlik Numarası")]
        public string CitizenshipNo { get; set; }
        [DisplayName("E-Posta")]
        public string Email { get; set; }
        [Required]
        [DisplayName("Telefon Numarası")]
        public string Phone { get; set; }
        [DisplayName("Diğer Bilgiler")]
        public string Other { get; set; }
        [DisplayName("Aktif mi?")]
        public bool IsActive { get; set; }

        [DisplayName("Oluşturulma Tarihi")]
        public DateTime CreatedAt { get; set; }
        [DisplayName("Oluşturan Kişi")]
        public string CreatedBy { get; set; }
        [DisplayName("Güncellenme Tarihi")]
        public DateTime? UpdatedAt { get; set; }
        [DisplayName("Güncelleyen Kişi")]
        public string UpdatedBy { get; set; }
        [DisplayName("Silinme Tarihi")]
        public DateTime? DeletedAt { get; set; }
        [DisplayName("Silen Kişi")]
        public string DeletedBy { get; set; }

        [JsonIgnore]
        public virtual List<Insurances> Insurances { get; set; }
    }
}
