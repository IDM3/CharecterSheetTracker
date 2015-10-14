using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharecterEntities
{
    [Flags]
    public enum SkillTraining
    {
        None = 0,
        Acrobatics = 1 << 0,
        AnimalHandling = 1 << 1,
        Arcana = 1 << 2,
        Athletics = 1 << 3,
        Deception = 1 << 4,
        History = 1 << 5,
        Insight = 1 << 6,
        Intimidation = 1 << 7,
        Investigation = 1 << 8,
        Medicine = 1 << 9,
        Nature = 1 << 10,
        Perception = 1 << 11,
        Performance = 1 << 12,
        Persuasion = 1 << 13,
        Religion = 1 << 14,
        SlieghtOfHand = 1 << 15,
        Stealth = 1 << 16,
        Survival = 1 << 17
    }
}
