using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharecterEntities
{
    public class Monster
    {
        public string Name { get; set; }
        public string Size { get; set; }
        public string Type { get; set; }
        public string Alignement { get; set; }
        public Abilities Ability { get; set; }
        public int ExpectedCR { get; set; }
        public int AC { get; set; }
        public string ArmorSource { get; set; }
        public int NumberOfHitDice { get; set; }
        public int SizeOfHitDice { get; set; }
        public List<string> Immunities { get; set; }
        public List<string> Resistances { get; set; }
        public SkillTraining Proficencies { get; set; }
    }
}
