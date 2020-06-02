using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DevService.Models
{
    public class Users
    {
        [Key]
        public string uLogin { get; set; }
        public string uPass { get; set; }
    }
}
