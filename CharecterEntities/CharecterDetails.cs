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
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class CharecterDetails
    {
        [Key]
        [JsonProperty]
        public int Id { get; set; }
        [JsonProperty]
        public string PlayerName { get; set; }
        [JsonProperty]
        public string Name { get; set; }
        [JsonProperty]
        public string Race { get; set; }
        [JsonProperty]
        public string Background { get; set; }
        [JsonProperty]
        public string Alignment { get; set; }
    }
}
