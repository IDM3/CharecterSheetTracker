using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CharecterEntities
{
    public class Overrides
    {
        public string PassivePerception { get; set; }
        public string Initiative { get; set; }
        public int ProfiencyBonus { get; set; }
    }
}
