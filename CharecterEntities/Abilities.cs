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
    public class Abilities
    {
        [Key]
        [JsonProperty]
        public int Id { get; set; }
        [JsonProperty]
        public int Strength { get; set; }
        [JsonProperty]
        public int Dexterity { get; set; }
        [JsonProperty]
        public int Constitution { get; set; }
        [JsonProperty]
        public int Intelligence { get; set; }
        [JsonProperty]
        public int Wisdom { get; set; }
        [JsonProperty]
        public int Charisma { get; set; }
    }
}
