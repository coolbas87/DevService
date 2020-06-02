using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace DevService.Models
{
    public class CommandParams
    {
        [Key]
        public int wscpID { get; set; }
        public int wscID { get; set; }
        public string pName { get; set; }
        public string pOurName { get; set; }
        [ForeignKey("wscID")]
        public Commands Command { get; set; }
    }
}
