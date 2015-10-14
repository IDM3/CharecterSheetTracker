using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using JsonDataHandler;

namespace CharecterEntities
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class Character : JsonDataHandler.IIndexEntity
    {
        [Key]
        [JsonProperty]
        public int Id { get; set; }
        [JsonProperty]
        public CharecterDetails Details { get; set; }
        [JsonProperty]
        public Dictionary<string, int> ClassLevels { get; set; }
        [JsonProperty]
        public long Experience { get; set; }
        [JsonProperty]
        public Abilities Abilities { get; set; }
        [JsonProperty]
        public AbilityTraining SavingThrows { get; set; }
        [JsonProperty]
        public AbilityTraining SavingThrowSpecialNote { get; set; }
        [JsonProperty]
        public SkillTraining SkillProfiencies { get; set; }
        [JsonProperty]
        public SkillTraining SkillExcellencies { get; set; }
        [JsonProperty]
        public SkillTraining SkillSpecialNote { get; set; }
        [JsonProperty]
        public int MaxHitPoints { get; set; }
        [JsonProperty]
        public string HitDice { get; set; }
        [DataType(DataType.MultilineText)]
        [JsonProperty]
        public string PersonalityTraits { get; set; }
        [DataType(DataType.MultilineText)]
        [JsonProperty]
        public string Ideals { get; set; }
        [DataType(DataType.MultilineText)]
        [JsonProperty]
        public string Bonds { get; set; }
        [DataType(DataType.MultilineText)]
        [JsonProperty]
        public string Flaws { get; set; }
        [DataType(DataType.MultilineText)]
        [JsonProperty]
        public string Features { get; set; }
        [DataType(DataType.MultilineText)]
        [JsonProperty]
        public string Traits { get; set; }
        [JsonProperty]
        public string Speed { get; set; }
        [JsonProperty]
        public List<string> OtherProfiencies { get; set; }
        [JsonProperty]
        public List<string> Languages { get; set; }
        [JsonProperty]
        public List<CharecterInventoryItem> Items { get; set; }
        [JsonProperty]
        public Overrides SpecialOverides { get; set; }
        [JsonProperty]
        public List<string> EpicBoons { get; set; }
        [JsonProperty]
        public List<string> CharecterHistory { get; set; }

        public Character()
        {
            if(Details == null)
            {
                Details = new CharecterDetails();
            }
            if (ClassLevels == null)
            {
                ClassLevels = new Dictionary<string, int>();
            }
            if(Abilities == null)
            {
                Abilities = new Abilities();
            }
            if(OtherProfiencies == null)
            {
                OtherProfiencies = new List<string>();
            }
            if(Languages == null)
            {
                Languages = new List<string>();
            }
            if(Items == null)
            {
                Items = new List<CharecterInventoryItem>();
            }
            if(SpecialOverides == null)
            {
                SpecialOverides = new Overrides();
            }
            if(EpicBoons == null)
            {
                EpicBoons = new List<string>();
            }
            if(CharecterHistory == null)
            {
                CharecterHistory = new List<string>();
            }
        }
    }
}
