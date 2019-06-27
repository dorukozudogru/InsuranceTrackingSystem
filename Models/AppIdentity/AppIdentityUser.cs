using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SigortaTakipSistemi.Models
{
    public class AppIdentityUser : IdentityUser
    {
        public DateTime CreatedAt;
        public DateTime? UpdatedAt;
        public DateTime? DeletedAt;
    }
}
