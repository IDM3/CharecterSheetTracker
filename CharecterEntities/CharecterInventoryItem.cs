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
    public class CharecterInventoryItem
    {
        [Key]
        [JsonProperty]
        public int Id { get; set; }
        [JsonProperty]
        public Item ItemHeld { get; set; }
        [JsonProperty]
        public bool Equipped { get; set; }
        [JsonProperty]
        public int Count { get; set; }
    }
}
