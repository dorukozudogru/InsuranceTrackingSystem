using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace SigortaTakipSistemi.Models
{
    public class AppIdentityUser : IdentityUser
    {
        //[DisplayName("Adı")]
        //public string Name { get; set; }
        //[DisplayName("Soyadı")]
        //public string Surname { get; set; }

        [DisplayName("Aktif Mi?")]
        public bool IsActive { get; set; }

        [DisplayName("Admin Mi?")]
        public bool IsAdmin { get; set; }
    }
}
