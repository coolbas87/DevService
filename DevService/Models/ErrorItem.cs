using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DevService.Models
{
    public class ErrorItem
    {
        public int CommandRes { get; set; }
        public string CommandError { get; set; }

        public ErrorItem(string commandError, int commandRes = -1)
        {
            CommandRes = commandRes;
            CommandError = commandError;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
