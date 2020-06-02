using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DevService.Models
{
    public partial class Commands
    {
        [Key]
        public int wscID { get; set; }
        public string cName { get; set; }
        public string ProcName { get; set; }
        public string Server { get; set; }
        public string Base { get; set; }
        public List<CommandParams> Params { get; set; }
    }
}
