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
    public class Item : JsonDataHandler.IIndexEntity
    {
        [Key]
        [JsonProperty]
        public int Id { get; set; }
        [JsonProperty]
        public string Name { get; set; }
        [JsonProperty]
        public string Damage { get; set; }
        [JsonProperty]
        public string Range { get; set; }
        [JsonProperty]
        public bool IsExpendable { get; set; }
        [JsonProperty]
        public int MaxDex { get; set; }
        [JsonProperty]
        public int BaseAc { get; set; }
        [JsonProperty]
        public string Type { get; set; }
        [JsonProperty]
        public bool StealthDisadvantage { get; set; }
        [JsonProperty]
        public bool Light { get; set; }
        [JsonProperty]
        public bool TwoHanded { get; set; }
        [JsonProperty]
        public bool Finesse { get; set; }
        [JsonProperty]
        public bool Thrown { get; set; }
        [JsonProperty]
        public bool Veristile { get; set; }
        [JsonProperty]
        public bool Heavy { get; set; }
        [JsonProperty]
        public bool Reach { get; set; }
        [JsonProperty]
        public bool Loading { get; set; }
        [JsonProperty]
        public bool Special { get; set; }
        [JsonProperty]
        public string VeristileDmg { get; set; }
        [JsonProperty]
        public float Weight { get; set; }
    }
}
