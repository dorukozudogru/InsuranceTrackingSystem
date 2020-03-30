using System;
using System.ComponentModel.DataAnnotations;

namespace SigortaTakipSistemi.Models
{
    public class Audit
    {
        [Key]
        public int Id { get; set; }
        public string TableName { get; set; }
        public string EntityName { get; set; }
        public string Action { get; set; }
        public DateTime DateTime { get; set; }
        public string Username { get; set; }
        public string KeyValues { get; set; }
        public string OldValues { get; set; }
        public string NewValues { get; set; }
    }
}