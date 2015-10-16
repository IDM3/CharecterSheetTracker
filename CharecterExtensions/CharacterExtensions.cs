using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JsonDataHandler;
using CharecterEntities;
using System.IO;
using System.Reflection;

namespace CharecterExtensions
{
    public static class CharecterExtensions
    {
        public static string DefaultCharecterData
        {
            get
            {
                string data;
                try
                {
                    Assembly assem = Assembly.GetExecutingAssembly();
                    var _textStreamReader = new StreamReader(assem.GetManifestResourceStream("CharecterExtensions.Character"));
                    data = _textStreamReader.ReadToEnd();
                    _textStreamReader.Close();
                }
                catch
                {
                    data = null;
                }
                return data;
            }
        }

        public static DataHandler<Character> CharecterProvider = new DataHandler<Character>(Common.folderBase, DefaultCharecterData);

        static CharecterExtensions()
        {
        }

        public static List<Character> LoadAll(this List<Character> characters)
        {
            characters = CharecterProvider.GetAll();
            characters.ForEach(c => c.Items.ForEach(i => i.ItemHeld.Load()));
            return characters;
        }

        public static Character Load(this Character characterToLoad)
        {
            return characterToLoad.Load(characterToLoad.Id);
        }

        public static Character Load(this Character characterToLoad, int idToPopulate)
        {
            characterToLoad = CharecterProvider.GetById(idToPopulate);
            characterToLoad.Items.ForEach(x => x.ItemHeld.Load());
            return characterToLoad;
        }

        public static Character Save(this Character characterToSave)
        {
            CharecterProvider.Save(characterToSave);
            return characterToSave;
        }

        public static void Delete(this Character character)
        {
            CharecterProvider.Delete(character.Id);
        }
        public static void Delete(this Character character, int idToDelete)
        {
            CharecterProvider.Delete(idToDelete);
        }

        public static int CharecterLevel(this Character character)
        {
            return character.ClassLevels.Values.Sum();
        }

        public static int ProfiencyBonus(this Character character)
        {
            int profiency;
            if(character.SpecialOverides != null && character.SpecialOverides.ProfiencyBonus != 0)
            {
                profiency = character.SpecialOverides.ProfiencyBonus;
            }
            else if(character.CharecterLevel() >= 17)
            {
                profiency = 6;
            }
            else if(character.CharecterLevel() >= 13)
            {
                profiency = 5;
            }
            else if (character.CharecterLevel() >= 9)
            {
                profiency = 4;
            }
            else if (character.CharecterLevel() >= 5)
            {
                profiency = 3;
            }
            else
            {
                profiency = 2;
            }
            return profiency;
        }

        public static int SkillRank(this Character character, SkillTraining training)
        {
            int rank = 0;
            switch(training)
            {
                case SkillTraining.Athletics:
                    rank = character.Abilities.Strength.Modifier();
                    break;
                case SkillTraining.Acrobatics:
                case SkillTraining.SlieghtOfHand:
                case SkillTraining.Stealth:
                    rank = character.Abilities.Dexterity.Modifier();
                    break;
                case SkillTraining.Arcana:
                case SkillTraining.History:
                case SkillTraining.Investigation:
                case SkillTraining.Nature:
                case SkillTraining.Religion:
                    rank = character.Abilities.Intelligence.Modifier();
                    break;
                case SkillTraining.AnimalHandling:
                case SkillTraining.Insight:
                case SkillTraining.Medicine:
                case SkillTraining.Perception:
                case SkillTraining.Survival:
                    rank = character.Abilities.Wisdom.Modifier();
                    break;
                case SkillTraining.Deception:
                case SkillTraining.Intimidation:
                case SkillTraining.Performance:
                case SkillTraining.Persuasion:
                    rank = character.Abilities.Charisma.Modifier();
                    break;
            }
            if(character.SkillProfiencies.HasFlag(training))
            {
                rank += character.ProfiencyBonus();
            }
            if (character.SkillExcellencies.HasFlag(training))
            {
                rank += character.ProfiencyBonus();
            }
            return rank;
        }

        public static int SavingThrow(this Character character, AbilityTraining training)
        {
            int rank = 0;
            switch (training)
            {
                case AbilityTraining.Strength:
                    rank = character.Abilities.Strength.Modifier();
                    break;
                case AbilityTraining.Dexterity:
                    rank = character.Abilities.Dexterity.Modifier();
                    break;
                case AbilityTraining.Constitution:
                    rank = character.Abilities.Constitution.Modifier();
                    break;
                case AbilityTraining.Intelligence:
                    rank = character.Abilities.Intelligence.Modifier();
                    break;
                case AbilityTraining.Wisdom:
                    rank = character.Abilities.Wisdom.Modifier();
                    break;
                case AbilityTraining.Charisma:
                    rank = character.Abilities.Charisma.Modifier();
                    break;
            }
            if (character.SavingThrows.HasFlag(training))
            {
                rank += character.ProfiencyBonus();
            }
            return rank;
        }

        public static string PassivePerception(this Character character)
        {
            string passivePerception;
            if(character.SpecialOverides != null && !string.IsNullOrWhiteSpace(character.SpecialOverides.PassivePerception))
            {
                passivePerception = character.SpecialOverides.PassivePerception;
            }
            else
            {
                passivePerception = (10 + character.SkillRank(SkillTraining.Perception)).ToString();
            }
            return passivePerception;
        }

        public static long XPToNextLevel(this Character character)
        {
            long xpNeededTotal = 0;
            for(int i = 1; i <= character.CharecterLevel(); i++)
            {
                xpNeededTotal += XPToLevel(i);
            }
            xpNeededTotal += character.EpicBoons.Count * 30000;
            return xpNeededTotal;
        }

        public static int AC(this Character character)
        {
            character.LoadItems();
            int ac = 10;
            var armorItems = character.Items.Where(i => i.ItemHeld.BaseAc != 0 && i.Equipped).Select(x => x.ItemHeld);
            if(armorItems.Any())
            {
                ac = armorItems.Sum(x => x.BaseAc);
                if(ac < 10)
                {
                    ac += 10;
                }
                var dex = character.Abilities.Dexterity.Modifier();
                var maxDex = armorItems.Min(x => x.MaxDex);
                if(armorItems.Any(i => i.Name.Contains("Monk")))
                {
                    ac += dex;
                    if(character.Abilities.Wisdom.Modifier() > 0)
                    {
                        ac += character.Abilities.Wisdom.Modifier();
                    }
                }
                else if (armorItems.Any(i => i.Name.Contains("Barbarian")))
                {
                    ac += dex;
                    if (character.Abilities.Constitution.Modifier() > 0)
                    {
                        ac += character.Abilities.Constitution.Modifier();
                    }
                }
                else
                {
                    if (dex < maxDex)
                    {
                        ac += dex;
                    }
                    else
                    {
                        ac += maxDex;
                    }
                }
            }
            else
            {
                ac += character.Abilities.Dexterity.Modifier();
            }
            return ac;
        }

        public static long XPToLevel(int currentLevel)
        {
            long xpToLevel;
            switch(currentLevel)
            {
                case 1:
                    xpToLevel = 300;
                    break;
                case 2:
                    xpToLevel = 600;
                    break;
                case 3:
                    xpToLevel = 1800;
                    break;
                case 4:
                    xpToLevel = 3800;
                    break;
                case 5:
                    xpToLevel = 7500;
                    break;
                case 6:
                    xpToLevel = 9000;
                    break;
                case 7:
                    xpToLevel = 11000;
                    break;
                case 8:
                    xpToLevel = 14000;
                    break;
                case 9:
                    xpToLevel = 16000;
                    break;
                case 10:
                    xpToLevel = 21000;
                    break;
                case 11:
                    xpToLevel = 15000;
                    break;
                case 12:
                    xpToLevel = 20000;
                    break;
                case 13:
                    xpToLevel = 20000;
                    break;
                case 14:
                    xpToLevel = 25000;
                    break;
                case 15:
                    xpToLevel = 30000;
                    break;
                case 16:
                    xpToLevel = 30000;
                    break;
                case 17:
                    xpToLevel = 40000;
                    break;
                case 18:
                    xpToLevel = 40000;
                    break;
                case 19:
                    xpToLevel = 50000;
                    break;
                default:
                    xpToLevel = 30000;
                    break;
            }
            return xpToLevel;
        }

        public static void LoadItems(this Character character)
        {
            character.Items.ForEach(i => i.ItemHeld = i.ItemHeld.Load());
        }

        private static string BoxRep(this int count)
        {
            string rep = string.Empty;
            for(int i = 0; i < count; i++)
            {
                rep += "[  ]";
            }
            return rep;
        }

        public static string GetPdf(this Character character, bool isAlt)
        {
            character.LoadItems();
            string charecterSheetLocation = Common.folderBase;
            if(isAlt)
            {
                charecterSheetLocation += "Character Sheet - Alternative.pdf";
            }
            else
            {
                charecterSheetLocation += "Character Sheet.pdf";
            }
            if(!File.Exists(charecterSheetLocation))
            {

                string resourceName = "CharecterExtensions.";
                if(isAlt)
                {
                    resourceName += "SheetAlternative .pdf";
                }
                else
                {
                    resourceName += "Sheet.pdf";
                }
                Assembly assem = Assembly.GetExecutingAssembly();
                var reader = new StreamReader(assem.GetManifestResourceStream(resourceName));
                var writer = new StreamWriter(charecterSheetLocation);
                byte[] buffer = new byte[32768];
                int read;
                while ((read = reader.BaseStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    writer.BaseStream.Write(buffer, 0, read);
                }
                writer.Flush();
                writer.Close();
                reader.Close();
            }

            Dictionary<string, string> formValues = new Dictionary<string, string>();
            formValues.Add("CharacterName", character.Details.Name);
            formValues.Add("PlayerName", character.Details.PlayerName);
            formValues.Add("Background", character.Details.Background);
            formValues.Add("Race", character.Details.Race);
            string classes = string.Join(", ", character.ClassLevels.Select(x => x.Key + " " + x.Value));
            formValues.Add("ClassLevel", classes);
            formValues.Add("Alignment", character.Details.Alignment);
            string xpRepresentation;
            if (character.Experience == 0)
            {
                xpRepresentation = "          /" + character.XPToNextLevel().ToString("#,#");
            }
            else
            {
                xpRepresentation = character.Experience.ToString("#,#") + "/" + character.XPToNextLevel().ToString("#,#");
            }
            formValues.Add("XP", xpRepresentation);
            formValues.Add("ProfBonus", character.ProfiencyBonus().ToString("+#;-#;0"));
            formValues.Add("STRmod", character.Abilities.Strength.ToString());
            formValues.Add("STR", character.Abilities.Strength.Modifier().ToString("+#;-#;0"));
            formValues.Add("DEXmod", character.Abilities.Dexterity.ToString());
            formValues.Add("DEX", character.Abilities.Dexterity.Modifier().ToString("+#;-#;0"));
            formValues.Add("CONmod", character.Abilities.Constitution.ToString());
            formValues.Add("CON", character.Abilities.Constitution.Modifier().ToString("+#;-#;0"));
            formValues.Add("INTmod", character.Abilities.Intelligence.ToString());
            formValues.Add("INT", character.Abilities.Intelligence.Modifier().ToString("+#;-#;0"));
            formValues.Add("WISmod", character.Abilities.Wisdom.ToString());
            formValues.Add("WIS", character.Abilities.Wisdom.Modifier().ToString("+#;-#;0"));
            formValues.Add("CHamod", character.Abilities.Charisma.ToString());
            formValues.Add("CHA", character.Abilities.Charisma.Modifier().ToString("+#;-#;0"));
            formValues.Add("Passive", character.PassivePerception());
            formValues.Add("ProficienciesLang", string.Join(", ", character.Languages) + Environment.NewLine + string.Join(", ", character.OtherProfiencies));
            formValues.Add("PersonalityTraits", character.PersonalityTraits);
            formValues.Add("Ideals", character.Ideals);
            formValues.Add("Bonds", character.Bonds);
            formValues.Add("Flaws", character.Flaws);
            formValues.Add("Features and Traits", (character.Features ?? string.Empty) + Environment.NewLine + (character.Traits ?? string.Empty));
            formValues.Add("SavingThrows", character.SavingThrow(AbilityTraining.Strength).ToString("+#;-#;0") + (character.SavingThrowSpecialNote.HasFlag(AbilityTraining.Strength) ? "*" : ""));
            formValues.Add("SavingThrows2", character.SavingThrow(AbilityTraining.Dexterity).ToString("+#;-#;0") + (character.SavingThrowSpecialNote.HasFlag(AbilityTraining.Dexterity) ? "*" : ""));
            formValues.Add("SavingThrows3", character.SavingThrow(AbilityTraining.Constitution).ToString("+#;-#;0") + (character.SavingThrowSpecialNote.HasFlag(AbilityTraining.Constitution) ? "*" : ""));
            formValues.Add("SavingThrows4", character.SavingThrow(AbilityTraining.Intelligence).ToString("+#;-#;0") + (character.SavingThrowSpecialNote.HasFlag(AbilityTraining.Intelligence) ? "*" : ""));
            formValues.Add("SavingThrows5", character.SavingThrow(AbilityTraining.Wisdom).ToString("+#;-#;0") + (character.SavingThrowSpecialNote.HasFlag(AbilityTraining.Wisdom) ? "*" : ""));
            formValues.Add("SavingThrows6", character.SavingThrow(AbilityTraining.Charisma).ToString("+#;-#;0") + (character.SavingThrowSpecialNote.HasFlag(AbilityTraining.Charisma) ? "*" : ""));
            formValues.Add("Athletics", character.SkillRank(SkillTraining.Athletics).ToString("+#;-#;0") + (character.SkillSpecialNote.HasFlag(SkillTraining.Athletics) ? "*" : ""));
            formValues.Add("Acrobatics", character.SkillRank(SkillTraining.Acrobatics).ToString("+#;-#;0") + (character.SkillSpecialNote.HasFlag(SkillTraining.Acrobatics) ? "*" : ""));
            formValues.Add("SleightofHand", character.SkillRank(SkillTraining.SlieghtOfHand).ToString("+#;-#;0") + (character.SkillSpecialNote.HasFlag(SkillTraining.SlieghtOfHand) ? "*" : ""));
            formValues.Add("Stealth", character.SkillRank(SkillTraining.Stealth).ToString("+#;-#;0") + (character.SkillSpecialNote.HasFlag(SkillTraining.Stealth) ? "*" : ""));
            formValues.Add("Arcana", character.SkillRank(SkillTraining.Arcana).ToString("+#;-#;0") + (character.SkillSpecialNote.HasFlag(SkillTraining.Arcana) ? "*" : ""));
            formValues.Add("History", character.SkillRank(SkillTraining.History).ToString("+#;-#;0") + (character.SkillSpecialNote.HasFlag(SkillTraining.History) ? "*" : ""));
            formValues.Add("Investigation", character.SkillRank(SkillTraining.Investigation).ToString("+#;-#;0") + (character.SkillSpecialNote.HasFlag(SkillTraining.Investigation) ? "*" : ""));
            formValues.Add("Nature", character.SkillRank(SkillTraining.Nature).ToString("+#;-#;0") + (character.SkillSpecialNote.HasFlag(SkillTraining.Nature) ? "*" : ""));
            formValues.Add("Religion", character.SkillRank(SkillTraining.Religion).ToString("+#;-#;0") + (character.SkillSpecialNote.HasFlag(SkillTraining.Religion) ? "*" : ""));
            formValues.Add("Animal", character.SkillRank(SkillTraining.AnimalHandling).ToString("+#;-#;0") + (character.SkillSpecialNote.HasFlag(SkillTraining.AnimalHandling) ? "*" : ""));
            formValues.Add("Insight", character.SkillRank(SkillTraining.Insight).ToString("+#;-#;0") + (character.SkillSpecialNote.HasFlag(SkillTraining.Insight) ? "*" : ""));
            formValues.Add("Medicine", character.SkillRank(SkillTraining.Medicine).ToString("+#;-#;0") + (character.SkillSpecialNote.HasFlag(SkillTraining.Medicine) ? "*" : ""));
            formValues.Add("Perception", character.SkillRank(SkillTraining.Perception).ToString("+#;-#;0") + (character.SkillSpecialNote.HasFlag(SkillTraining.Perception) ? "*" : ""));
            formValues.Add("Survival", character.SkillRank(SkillTraining.Survival).ToString("+#;-#;0") + (character.SkillSpecialNote.HasFlag(SkillTraining.Survival) ? "*" : ""));
            formValues.Add("Deception", character.SkillRank(SkillTraining.Deception).ToString("+#;-#;0") + (character.SkillSpecialNote.HasFlag(SkillTraining.Deception) ? "*" : ""));
            formValues.Add("Intimidation", character.SkillRank(SkillTraining.Intimidation).ToString("+#;-#;0") + (character.SkillSpecialNote.HasFlag(SkillTraining.Intimidation) ? "*" : ""));
            formValues.Add("Performance", character.SkillRank(SkillTraining.Performance).ToString("+#;-#;0") + (character.SkillSpecialNote.HasFlag(SkillTraining.Performance) ? "*" : ""));
            formValues.Add("Persuasion", character.SkillRank(SkillTraining.Persuasion).ToString("+#;-#;0") + (character.SkillSpecialNote.HasFlag(SkillTraining.Persuasion) ? "*" : ""));
            formValues.Add("HDTotal", character.HitDice);
            formValues.Add("HPMax", character.MaxHitPoints.ToString());
            string initiative;
            if(string.IsNullOrWhiteSpace(character.SpecialOverides.Initiative))
            {
                initiative = character.Abilities.Dexterity.Modifier().ToString("+#;-#;0");
            }
            else
            {
                initiative = character.SpecialOverides.Initiative ?? string.Empty;
            }
            formValues.Add("Speed", character.Speed);
            formValues.Add("AC", character.AC().ToString());
            formValues.Add("Initiative", initiative);
            var equippedWeapons = character.Items.Where(item => item.Equipped && !string.IsNullOrWhiteSpace(item.ItemHeld.Damage)).ToList();
            var ammunitionItems = character.Items.Where(item => item.ItemHeld.IsExpendable && item.Count > 10);
            Func<int, string> getWeaponName = (weaponIndex) =>
                {
                    string name;
                    if (equippedWeapons.Count() > weaponIndex)
                    {
                        if (equippedWeapons[weaponIndex].Count > 1 && equippedWeapons[weaponIndex].ItemHeld.Thrown)
                        {
                            name = equippedWeapons[weaponIndex].ItemHeld.Name + equippedWeapons[weaponIndex].Count.BoxRep();
                        }
                        else if (!string.IsNullOrWhiteSpace(equippedWeapons[weaponIndex].ItemHeld.Range))
                        {
                            name = equippedWeapons[weaponIndex].ItemHeld.Name + "(" + equippedWeapons[weaponIndex].ItemHeld.Range + ")";
                        }
                        else
                        {
                            name = equippedWeapons[weaponIndex].ItemHeld.Name;
                        }
                    }
                    else
                    {
                        name = string.Empty;
                    }
                    return name;
                };
            Func<int, string> getWeaponAttack = (weaponIndex) =>
                {
                    string weaponAttack;
                    if (equippedWeapons.Count() > weaponIndex)
                    {
                        weaponAttack = equippedWeapons[weaponIndex].ItemHeld.AttackModifier(character);
                    }
                    else
                    {
                        weaponAttack = string.Empty;
                    }
                    return weaponAttack;
                };
            Func<int, string> getWeaponDamage = (weaponIndex) =>
                {
                    string dmg;
                    if (equippedWeapons.Count() > weaponIndex)
                    {
                        if (equippedWeapons[weaponIndex].Count > 1 && equippedWeapons[weaponIndex].ItemHeld.Thrown)
                        {
                            if (equippedWeapons[weaponIndex].ItemHeld.Veristile)
                            {
                                dmg = equippedWeapons[weaponIndex].ItemHeld.Damage
                                    + "(" + equippedWeapons[weaponIndex].ItemHeld.VeristileDmg + ")"
                                    + equippedWeapons[weaponIndex].ItemHeld.AttackModifier(character, false)
                                    + "(" + equippedWeapons[weaponIndex].ItemHeld.Range + ")";
                            }
                            else
                            {
                                dmg = equippedWeapons[weaponIndex].ItemHeld.Damage
                                + equippedWeapons[weaponIndex].ItemHeld.AttackModifier(character, false)
                                + "(" + equippedWeapons[weaponIndex].ItemHeld.Range + ")";
                            }
                        }
                        else if (equippedWeapons[weaponIndex].ItemHeld.Veristile)
                        {
                            dmg = equippedWeapons[weaponIndex].ItemHeld.Damage
                                + "(" + equippedWeapons[weaponIndex].ItemHeld.VeristileDmg + ")"
                                + equippedWeapons[weaponIndex].ItemHeld.AttackModifier(character, false);
                        }
                        else
                        {
                            dmg = equippedWeapons[weaponIndex].ItemHeld.Damage + equippedWeapons[weaponIndex].ItemHeld.AttackModifier(character, false);
                        }
                    }
                    else
                    {
                        dmg = string.Empty;
                    }
                    return dmg;
                };
            string weaponName1 = getWeaponName(0);
            string weaponAtk1 = getWeaponAttack(0);
            string weaponDmg1 = getWeaponDamage(0);
            string weaponName2 = getWeaponName(1);
            string weaponAtk2 = getWeaponAttack(1);
            string weaponDmg2 = getWeaponDamage(1);
            string weaponName3 = getWeaponName(2);
            string weaponAtk3 = getWeaponAttack(2);
            string weaponDmg3 = getWeaponDamage(2);
            formValues.Add("Wpn Name", weaponName1);
            formValues.Add("Wpn1 AtkBonus", weaponAtk1);
            formValues.Add("Wpn1 Damage", weaponAtk1);
            formValues.Add("Wpn Name 2", weaponName2);
            formValues.Add("Wpn2 AtkBonus", weaponAtk2);
            formValues.Add("Wpn2 Damage", weaponDmg2);
            formValues.Add("Wpn Name 3", weaponName3);
            formValues.Add("Wpn3 AtkBonus", weaponAtk3);
            formValues.Add("Wpn3 Damage", weaponDmg3);
            string additionalData = string.Empty;
            if (equippedWeapons.Count() > 3)
            {
                for (int equippedWeaponIndex = 3; equippedWeaponIndex < equippedWeapons.Count(); equippedWeaponIndex++)
                {
                    Item equippedWeaponToDisplay = equippedWeapons[equippedWeaponIndex].ItemHeld;
                    additionalData += equippedWeaponToDisplay.Name;
                    if (equippedWeaponToDisplay.Thrown)
                    {
                        additionalData += equippedWeapons[equippedWeaponIndex].Count.BoxRep();
                    }
                    if (!string.IsNullOrWhiteSpace(equippedWeaponToDisplay.Range))
                    {
                        additionalData += "(" + equippedWeaponToDisplay.Range + ")";
                    }
                    additionalData += " " + equippedWeaponToDisplay.AttackModifier(character);
                    additionalData += "(";
                    if (equippedWeaponToDisplay.Veristile)
                    {
                        additionalData += "[" + equippedWeaponToDisplay.Damage + "|"
                            + equippedWeaponToDisplay.VeristileDmg + "]";
                    }
                    else
                    {
                        additionalData += equippedWeaponToDisplay.Damage;
                    }
                    additionalData += equippedWeaponToDisplay.AttackModifier(character, false) + ")";
                    additionalData += Environment.NewLine;
                }
            }
            if (ammunitionItems.Any())
            {
                foreach (var ammunitionItem in ammunitionItems)
                {
                    additionalData += ammunitionItem.ItemHeld.Name;
                    additionalData += Environment.NewLine;
                    for (int co = 1; co < ammunitionItem.Count; )
                    {
                        for (int co2 = 0; co2 < 10; co2++)
                        {
                            additionalData += "[  ]";
                            co++;
                            if (co > ammunitionItem.Count)
                            {
                                break;
                            }
                        }
                        additionalData += Environment.NewLine;
                    }
                }
            }
            additionalData = additionalData.Trim();
            formValues.Add("AttacksSpellcasting", additionalData);
            string equipment = string.Join(", ", character.Items.Where(item =>
                {
                    bool showItemInEquipment = true;
                    if (item.ItemHeld.IsExpendable && item.Count > 10)
                    {
                        showItemInEquipment = false;
                    }
                    else if (item.Equipped && !string.IsNullOrWhiteSpace(item.ItemHeld.Damage))
                    {
                        showItemInEquipment = false;
                    }
                    else if (item.ItemHeld.Name.Contains("DO NOT SHOW"))
                    {
                        showItemInEquipment = false;
                    }
                    return showItemInEquipment;
                }).Select(x =>
                {
                    string item = x.ItemHeld.Name;
                    if (x.ItemHeld.IsExpendable)
                    {
                        for (int count = 0; count < x.Count; count++)
                        {
                            item = "[  ]" + item;
                        }
                    }
                    else if (x.Count > 1)
                    {
                        item = x.Count + " " + item;
                    }
                    else
                    {
                        //just item;
                    }
                    return item;
                }));
            formValues.Add("Equipment", equipment);
            Dictionary<string, bool> checks = new Dictionary<string, bool>();
            checks.Add("ST Strength", character.SavingThrows.HasFlag(AbilityTraining.Strength));
            checks.Add("ST Dexterity", character.SavingThrows.HasFlag(AbilityTraining.Dexterity));
            checks.Add("ST Constitution", character.SavingThrows.HasFlag(AbilityTraining.Constitution));
            checks.Add("ST Intelligence", character.SavingThrows.HasFlag(AbilityTraining.Intelligence));
            checks.Add("ST Wisdom", character.SavingThrows.HasFlag(AbilityTraining.Wisdom));
            checks.Add("ST Charisma", character.SavingThrows.HasFlag(AbilityTraining.Charisma));
            checks.Add("ChBx Athletics", character.SkillProfiencies.HasFlag(SkillTraining.Athletics));
            checks.Add("ChBx Acrobatics", character.SkillProfiencies.HasFlag(SkillTraining.Acrobatics));
            checks.Add("ChBx Sleight", character.SkillProfiencies.HasFlag(SkillTraining.SlieghtOfHand));
            checks.Add("ChBx Stealth", character.SkillProfiencies.HasFlag(SkillTraining.Stealth));
            checks.Add("ChBx Arcana", character.SkillProfiencies.HasFlag(SkillTraining.Arcana));
            checks.Add("ChBx History", character.SkillProfiencies.HasFlag(SkillTraining.History));
            checks.Add("ChBx Investigation", character.SkillProfiencies.HasFlag(SkillTraining.Investigation));
            checks.Add("ChBx Nature", character.SkillProfiencies.HasFlag(SkillTraining.Nature));
            checks.Add("ChBx Religion", character.SkillProfiencies.HasFlag(SkillTraining.Religion));
            checks.Add("ChBx Animal", character.SkillProfiencies.HasFlag(SkillTraining.AnimalHandling));
            checks.Add("ChBx Insight", character.SkillProfiencies.HasFlag(SkillTraining.Insight));
            checks.Add("ChBx Medicine", character.SkillProfiencies.HasFlag(SkillTraining.Medicine));
            checks.Add("ChBx Perception", character.SkillProfiencies.HasFlag(SkillTraining.Perception));
            checks.Add("ChBx Survival", character.SkillProfiencies.HasFlag(SkillTraining.Survival));
            checks.Add("ChBx Deception", character.SkillProfiencies.HasFlag(SkillTraining.Deception));
            checks.Add("ChBx Intimidation", character.SkillProfiencies.HasFlag(SkillTraining.Intimidation));
            checks.Add("ChBx Performance", character.SkillProfiencies.HasFlag(SkillTraining.Performance));
            checks.Add("ChBx Persuasion", character.SkillProfiencies.HasFlag(SkillTraining.Persuasion));

            string fileType = charecterSheetLocation.Split('\\').Last().Trim();
            string fileName = Common.folderBase + character.Details.Name + "-Level " + character.CharecterLevel() + "-" + fileType;
            ItextSharperWrapper.PdfFormFiller filler = new ItextSharperWrapper.PdfFormFiller(charecterSheetLocation);
            filler.FillForm(fileName, formValues, checks);
            return fileName;
        }
    }
}
