using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SigortaTakipSistemi.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Key]
        public int Id { get; set; }
        [DisplayName("Kullanıcı Adı")]
        public string UserName { get; set; }
        [DisplayName("E-Posta")]
        public string Email { get; set; }
        [DisplayName("Şifre")]
        public string Password { get; set; }
        public string Token { get; set; }
        [DisplayName("Aktif Mi?")]
        public bool IsActive { get; set; }
    }
}
