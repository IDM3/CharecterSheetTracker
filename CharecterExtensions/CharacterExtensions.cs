using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JsonDataHandler;
using CharecterEntities;
using Spire.Pdf;
using Spire.Pdf.Widget;
using Spire.Pdf.Fields;
using Spire.Pdf.Graphics;
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
            PdfDocument doc = new PdfDocument();
            doc.LoadFromFile(charecterSheetLocation);
            PdfFormWidget formWidget = doc.Form as PdfFormWidget;
            var equippedWeapons = character.Items.Where(item => item.Equipped && !string.IsNullOrWhiteSpace(item.ItemHeld.Damage)).ToList();
            var ammunitionItems = character.Items.Where(item => item.ItemHeld.IsExpendable && item.Count > 10);
            for(int i = 0; i < formWidget.FieldsWidget.List.Count; i++)
            {
                PdfField field = formWidget.FieldsWidget.List[i] as PdfField;
                if(field is PdfTextBoxFieldWidget)
                {
                    PdfTextBoxFieldWidget textBox = field as PdfTextBoxFieldWidget;
                    switch(textBox.Name.Trim())
                    {
                        case "CharacterName":
                            textBox.Text = character.Details.Name ?? string.Empty;
                            break;
                        case "PlayerName":
                            textBox.Text = character.Details.PlayerName ?? string.Empty;
                            break;
                        case "Background":
                            textBox.Text = character.Details.Background ?? string.Empty;
                            break;
                        case "Race":
                            textBox.Text = character.Details.Race ?? string.Empty;
                            break;
                        case "ClassLevel":
                            string classes = string.Join(", ", character.ClassLevels.Select(x => x.Key + " " + x.Value));
                            textBox.Text = classes;
                            break;
                        case "Alignment":
                            textBox.Text = character.Details.Alignment ?? string.Empty;
                            break;
                        case "XP":
                            if(character.Experience == 0)
                            {
                                textBox.Text = "          /" + character.XPToNextLevel().ToString("#,#");
                            }
                            else
                            {
                                textBox.Text = character.Experience.ToString("#,#") + "/" + character.XPToNextLevel().ToString("#,#");
                            }
                            break;
                        case "ProfBonus":
                            textBox.Text = character.ProfiencyBonus().ToString("+#;-#;0");
                            break;
                        case "STRmod":
                            textBox.Text = character.Abilities.Strength.ToString();
                            break;
                        case "STR":
                            textBox.Text = character.Abilities.Strength.Modifier().ToString("+#;-#;0");
                            break;
                        case "DEXmod":
                            textBox.Text = character.Abilities.Dexterity.ToString();
                            break;
                        case "DEX":
                            textBox.Text = character.Abilities.Dexterity.Modifier().ToString("+#;-#;0");
                            break;
                        case "CONmod":
                            textBox.Text = character.Abilities.Constitution.ToString();
                            break;
                        case "CON":
                            textBox.Text = character.Abilities.Constitution.Modifier().ToString("+#;-#;0");
                            break;
                        case "INTmod":
                            textBox.Text = character.Abilities.Intelligence.ToString();
                            break;
                        case "INT":
                            textBox.Text = character.Abilities.Intelligence.Modifier().ToString("+#;-#;0");
                            break;
                        case "WISmod":
                            textBox.Text = character.Abilities.Wisdom.ToString();
                            break;
                        case "WIS":
                            textBox.Text = character.Abilities.Wisdom.Modifier().ToString("+#;-#;0");
                            break;
                        case "CHamod":
                            textBox.Text = character.Abilities.Charisma.ToString();
                            break;
                        case "CHA":
                            textBox.Text = character.Abilities.Charisma.Modifier().ToString("+#;-#;0");
                            break;
                        case "Passive":
                            textBox.Text = character.PassivePerception();
                            break;
                        case "ProficienciesLang":
                            textBox.Text = string.Join(", ", character.Languages) + Environment.NewLine + string.Join(", ", character.OtherProfiencies);
                            break;
                        case "PersonalityTraits":
                            textBox.Text = character.PersonalityTraits ?? string.Empty;
                            break;
                        case "Ideals":
                            textBox.Text = character.Ideals ?? string.Empty;
                            break;
                        case "Bonds":
                            textBox.Text = character.Bonds ?? string.Empty;
                            break;
                        case "Flaws":
                            textBox.Text = character.Flaws ?? string.Empty;
                            break;
                        case "Features and Traits":
                            textBox.Text = (character.Features ?? string.Empty) + Environment.NewLine + (character.Traits ?? string.Empty);
                            break;
                        case "SavingThrows":
                            textBox.Text = character.SavingThrow(AbilityTraining.Strength).ToString("+#;-#;0") + (character.SavingThrowSpecialNote.HasFlag(AbilityTraining.Strength) ? "*" : "");;
                            break;
                        case "SavingThrows2":
                            textBox.Text = character.SavingThrow(AbilityTraining.Dexterity).ToString("+#;-#;0") + (character.SavingThrowSpecialNote.HasFlag(AbilityTraining.Dexterity) ? "*" : "");;
                            break;
                        case "SavingThrows3":
                            textBox.Text = character.SavingThrow(AbilityTraining.Constitution).ToString("+#;-#;0") + (character.SavingThrowSpecialNote.HasFlag(AbilityTraining.Constitution) ? "*" : "");;
                            break;
                        case "SavingThrows4":
                            textBox.Text = character.SavingThrow(AbilityTraining.Intelligence).ToString("+#;-#;0") + (character.SavingThrowSpecialNote.HasFlag(AbilityTraining.Intelligence) ? "*" : "");;
                            break;
                        case "SavingThrows5":
                            textBox.Text = character.SavingThrow(AbilityTraining.Wisdom).ToString("+#;-#;0") + (character.SavingThrowSpecialNote.HasFlag(AbilityTraining.Wisdom) ? "*" : "");;
                            break;
                        case "SavingThrows6":
                            textBox.Text = character.SavingThrow(AbilityTraining.Charisma).ToString("+#;-#;0") + (character.SavingThrowSpecialNote.HasFlag(AbilityTraining.Charisma) ? "*" : "");;
                            break;
                        case "Athletics":
                            textBox.Text = character.SkillRank(SkillTraining.Athletics).ToString("+#;-#;0") + (character.SkillSpecialNote.HasFlag(SkillTraining.Athletics) ? "*" : ""); ;
                            break;
                        case "Acrobatics":
                            textBox.Text = character.SkillRank(SkillTraining.Acrobatics).ToString("+#;-#;0") + (character.SkillSpecialNote.HasFlag(SkillTraining.Acrobatics) ? "*" : ""); ;
                            break;
                        case "SleightofHand":
                            textBox.Text = character.SkillRank(SkillTraining.SlieghtOfHand).ToString("+#;-#;0") + (character.SkillSpecialNote.HasFlag(SkillTraining.SlieghtOfHand) ? "*" : ""); ;
                            break;
                        case "Stealth":
                            textBox.Text = character.SkillRank(SkillTraining.Stealth).ToString("+#;-#;0") + (character.SkillSpecialNote.HasFlag(SkillTraining.Stealth) ? "*" : ""); ;
                            break;
                        case "Arcana":
                            textBox.Text = character.SkillRank(SkillTraining.Arcana).ToString("+#;-#;0") + (character.SkillSpecialNote.HasFlag(SkillTraining.Arcana) ? "*" : ""); ;
                            break;
                        case "History":
                            textBox.Text = character.SkillRank(SkillTraining.History).ToString("+#;-#;0") + (character.SkillSpecialNote.HasFlag(SkillTraining.History) ? "*" : ""); ;
                            break;
                        case "Investigation":
                            textBox.Text = character.SkillRank(SkillTraining.Investigation).ToString("+#;-#;0") + (character.SkillSpecialNote.HasFlag(SkillTraining.Investigation) ? "*" : ""); ;
                            break;
                        case "Nature":
                            textBox.Text = character.SkillRank(SkillTraining.Nature).ToString("+#;-#;0") + (character.SkillSpecialNote.HasFlag(SkillTraining.Nature) ? "*" : ""); ;
                            break;
                        case "Religion":
                            textBox.Text = character.SkillRank(SkillTraining.Religion).ToString("+#;-#;0") + (character.SkillSpecialNote.HasFlag(SkillTraining.Religion) ? "*" : ""); ;
                            break;
                        case "Animal":
                            textBox.Text = character.SkillRank(SkillTraining.AnimalHandling).ToString("+#;-#;0") + (character.SkillSpecialNote.HasFlag(SkillTraining.AnimalHandling) ? "*" : ""); ;
                            break;
                        case "Insight":
                            textBox.Text = character.SkillRank(SkillTraining.Insight).ToString("+#;-#;0") + (character.SkillSpecialNote.HasFlag(SkillTraining.Insight) ? "*" : ""); ;
                            break;
                        case "Medicine":
                            textBox.Text = character.SkillRank(SkillTraining.Medicine).ToString("+#;-#;0") + (character.SkillSpecialNote.HasFlag(SkillTraining.Medicine) ? "*" : ""); ;
                            break;
                        case "Perception":
                            textBox.Text = character.SkillRank(SkillTraining.Perception).ToString("+#;-#;0") + (character.SkillSpecialNote.HasFlag(SkillTraining.Perception) ? "*" : ""); ;
                            break;
                        case "Survival":
                            textBox.Text = character.SkillRank(SkillTraining.Survival).ToString("+#;-#;0") + (character.SkillSpecialNote.HasFlag(SkillTraining.Survival) ? "*" : ""); ;
                            break;
                        case "Deception":
                            textBox.Text = character.SkillRank(SkillTraining.Deception).ToString("+#;-#;0") + (character.SkillSpecialNote.HasFlag(SkillTraining.Deception) ? "*" : ""); ;
                            break;
                        case "Intimidation":
                            textBox.Text = character.SkillRank(SkillTraining.Intimidation).ToString("+#;-#;0") + (character.SkillSpecialNote.HasFlag(SkillTraining.Intimidation) ? "*" : ""); ;
                            break;
                        case "Performance":
                            textBox.Text = character.SkillRank(SkillTraining.Performance).ToString("+#;-#;0") + (character.SkillSpecialNote.HasFlag(SkillTraining.Performance) ? "*" : ""); ;
                            break;
                        case "Persuasion":
                            textBox.Text = character.SkillRank(SkillTraining.Persuasion).ToString("+#;-#;0") + (character.SkillSpecialNote.HasFlag(SkillTraining.Persuasion) ? "*" : ""); ;
                            break;
                        case "HDTotal":
                            textBox.Text = character.HitDice ?? string.Empty;
                            break;
                        case "HPMax":
                            textBox.Text = character.MaxHitPoints.ToString();
                            break;
                        case "Initiative":
                            if(string.IsNullOrWhiteSpace(character.SpecialOverides.Initiative))
                            {
                                textBox.Text = character.Abilities.Dexterity.Modifier().ToString("+#;-#;0");
                            }
                            else
                            {
                                textBox.Text = character.SpecialOverides.Initiative ?? string.Empty;
                            }                            
                            break;
                        case "Speed":
                            textBox.Text = character.Speed ?? string.Empty;
                            break;
                        case "AC":
                            textBox.Text = character.AC().ToString();
                            break;
                        case "Wpn Name":
                            if(equippedWeapons.Count() > 0)
                            {
                                if (equippedWeapons[0].Count > 1 && equippedWeapons[0].ItemHeld.Thrown)
                                {
                                    textBox.Text = equippedWeapons[0].ItemHeld.Name + equippedWeapons[0].Count.BoxRep();
                                }
                                else if (!string.IsNullOrWhiteSpace(equippedWeapons[0].ItemHeld.Range))
                                {
                                    textBox.Text = equippedWeapons[0].ItemHeld.Name + "(" + equippedWeapons[0].ItemHeld.Range +")";
                                }
                                else
                                {
                                    textBox.Text = equippedWeapons[0].ItemHeld.Name;
                                }
                            }
                            break;
                        case "Wpn1 AtkBonus":
                            if (equippedWeapons.Count() > 0)
                            {
                                textBox.Text = equippedWeapons[0].ItemHeld.AttackModifier(character);
                            }
                            break;
                        case "Wpn1 Damage":
                            if (equippedWeapons.Count() > 0)
                            {
                                if (equippedWeapons[0].Count > 1 && equippedWeapons[0].ItemHeld.Thrown)
                                {
                                    if (equippedWeapons[0].ItemHeld.Veristile)
                                    {
                                        textBox.Text = equippedWeapons[0].ItemHeld.Damage
                                            + "(" + equippedWeapons[0].ItemHeld.VeristileDmg + ")"
                                            + equippedWeapons[0].ItemHeld.AttackModifier(character, false)
                                            + "(" + equippedWeapons[0].ItemHeld.Range + ")";
                                    }
                                    else
                                    {
                                        textBox.Text = equippedWeapons[0].ItemHeld.Damage
                                        + equippedWeapons[0].ItemHeld.AttackModifier(character, false)
                                        + "(" + equippedWeapons[0].ItemHeld.Range + ")";
                                    }
                                }
                                else if (equippedWeapons[0].ItemHeld.Veristile)
                                {
                                    textBox.Text = equippedWeapons[0].ItemHeld.Damage
                                        + "(" + equippedWeapons[0].ItemHeld.VeristileDmg + ")"
                                        + equippedWeapons[0].ItemHeld.AttackModifier(character, false);
                                }
                                else
                                {
                                    textBox.Text = equippedWeapons[0].ItemHeld.Damage + equippedWeapons[0].ItemHeld.AttackModifier(character, false);
                                }
                            }
                            break;
                        case "Wpn Name 2":
                            if (equippedWeapons.Count() > 1)
                            {
                                if (equippedWeapons[1].Count > 1 && equippedWeapons[1].ItemHeld.Thrown)
                                {
                                    textBox.Text = equippedWeapons[1].ItemHeld.Name + equippedWeapons[1].Count.BoxRep();
                                }
                                else if (!string.IsNullOrWhiteSpace(equippedWeapons[1].ItemHeld.Range))
                                {
                                    textBox.Text = equippedWeapons[1].ItemHeld.Name + "(" + equippedWeapons[1].ItemHeld.Range + ")";
                                }
                                else
                                {
                                    textBox.Text = equippedWeapons[1].ItemHeld.Name;
                                }
                            }
                            break;
                        case "Wpn2 AtkBonus":
                            if (equippedWeapons.Count() > 1)
                            {
                                textBox.Text = equippedWeapons[1].ItemHeld.AttackModifier(character);
                            }
                            break;
                        case "Wpn2 Damage":
                            if (equippedWeapons.Count() > 1)
                            {
                                if (equippedWeapons[1].Count > 1 && equippedWeapons[1].ItemHeld.Thrown)
                                {
                                    if (equippedWeapons[1].ItemHeld.Veristile)
                                    {
                                        textBox.Text = equippedWeapons[1].ItemHeld.Damage
                                            + "(" + equippedWeapons[1].ItemHeld.VeristileDmg + ")"
                                            + equippedWeapons[1].ItemHeld.AttackModifier(character, false)
                                            + "(" + equippedWeapons[1].ItemHeld.Range + ")";
                                    }
                                    else
                                    {
                                        textBox.Text = equippedWeapons[1].ItemHeld.Damage
                                        + equippedWeapons[1].ItemHeld.AttackModifier(character, false)
                                        + "(" + equippedWeapons[1].ItemHeld.Range + ")";
                                    }
                                }
                                else if(equippedWeapons[1].ItemHeld.Veristile)
                                {
                                    textBox.Text = equippedWeapons[1].ItemHeld.Damage 
                                        + "(" + equippedWeapons[1].ItemHeld.VeristileDmg + ")"
                                        + equippedWeapons[1].ItemHeld.AttackModifier(character, false);
                                }
                                else
                                {
                                    textBox.Text = equippedWeapons[1].ItemHeld.Damage + equippedWeapons[1].ItemHeld.AttackModifier(character, false);
                                }
                            }
                            break;
                        case "Wpn Name 3":
                            if (equippedWeapons.Count() > 2)
                            {
                                if (equippedWeapons[2].Count > 1 && equippedWeapons[2].ItemHeld.Thrown)
                                {
                                    textBox.Text = equippedWeapons[2].ItemHeld.Name + equippedWeapons[2].Count.BoxRep();
                                }
                                else if (!string.IsNullOrWhiteSpace(equippedWeapons[2].ItemHeld.Range))
                                {
                                    textBox.Text = equippedWeapons[2].ItemHeld.Name + "(" + equippedWeapons[2].ItemHeld.Range + ")";
                                }
                                else
                                {
                                    textBox.Text = equippedWeapons[2].ItemHeld.Name;
                                }
                            }
                            break;
                        case "Wpn3 AtkBonus":
                            if (equippedWeapons.Count() > 2)
                            {
                                textBox.Text = equippedWeapons[2].ItemHeld.AttackModifier(character);
                            }
                            break;
                        case "Wpn3 Damage":
                            if (equippedWeapons.Count() > 2)
                            {
                                if (equippedWeapons[2].Count > 1 && equippedWeapons[2].ItemHeld.Thrown)
                                {
                                    if(equippedWeapons[2].ItemHeld.Veristile)
                                    {
                                        textBox.Text = equippedWeapons[2].ItemHeld.Damage
                                            + "(" + equippedWeapons[2].ItemHeld.VeristileDmg + ")"
                                            + equippedWeapons[2].ItemHeld.AttackModifier(character, false)
                                            + "(" + equippedWeapons[2].ItemHeld.Range + ")";
                                    }
                                    else
                                    {
                                        textBox.Text = equippedWeapons[2].ItemHeld.Damage
                                        + equippedWeapons[2].ItemHeld.AttackModifier(character, false)
                                        + "(" + equippedWeapons[2].ItemHeld.Range + ")";
                                    }
                                }
                                else if (equippedWeapons[2].ItemHeld.Veristile)
                                {
                                    textBox.Text = equippedWeapons[2].ItemHeld.Damage
                                        + "(" + equippedWeapons[2].ItemHeld.VeristileDmg + ")"
                                        + equippedWeapons[2].ItemHeld.AttackModifier(character, false);
                                }
                                else
                                {
                                    textBox.Text = equippedWeapons[2].ItemHeld.Damage + equippedWeapons[2].ItemHeld.AttackModifier(character, false);
                                }
                                
                            }
                            break;
                        case "AttacksSpellcasting":
                            string additionalData = string.Empty;
                            if(equippedWeapons.Count() > 3)
                            {
                                for (int equippedWeaponIndex = 3; equippedWeaponIndex < equippedWeapons.Count(); equippedWeaponIndex++)
                                {
                                    Item equippedWeaponToDisplay = equippedWeapons[equippedWeaponIndex].ItemHeld;
                                    additionalData += equippedWeaponToDisplay.Name;
                                    if (equippedWeaponToDisplay.Thrown)
                                    {
                                        additionalData += equippedWeapons[equippedWeaponIndex].Count.BoxRep();
                                    }
                                    if(!string.IsNullOrWhiteSpace(equippedWeaponToDisplay.Range))
                                    {
                                        additionalData += "(" + equippedWeaponToDisplay.Range + ")";
                                    }
                                    additionalData += " " + equippedWeaponToDisplay.AttackModifier(character);
                                    additionalData += "(";
                                    if(equippedWeaponToDisplay.Veristile)
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
                                foreach(var ammunitionItem in ammunitionItems)
                                {
                                    additionalData += ammunitionItem.ItemHeld.Name;
                                    additionalData += Environment.NewLine;
                                    for(int co = 1; co < ammunitionItem.Count; )
                                    {
                                        for (int co2 = 0; co2 < 10; co2++)
                                        {
                                            additionalData += "[  ]";
                                            co++;
                                            if(co > ammunitionItem.Count)
                                            {
                                                break;
                                            }
                                        }
                                        additionalData += Environment.NewLine;
                                    }
                                }
                            }
                            textBox.Text = additionalData.Trim();
                            break;
                        case "Equipment":
                            textBox.Text = string.Join(", ", character.Items.Where(item =>
                                {
                                    bool showItemInEquipment = true;
                                    if(item.ItemHeld.IsExpendable && item.Count > 10)
                                    {
                                        showItemInEquipment = false;
                                    }
                                    else if(item.Equipped && !string.IsNullOrWhiteSpace(item.ItemHeld.Damage))
                                    {
                                        showItemInEquipment = false;
                                    }
                                    else if(item.ItemHeld.Name.Contains("DO NOT SHOW"))
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
                            break;
                        case "HPCurrent":
                        case "HPTemp":
                        case "HD":
                        case "CP":
                        case "SP":
                        case "EP":
                        case "GP":
                        case "PP":
                        case "Inspiration":
                            //note keeping area
                            break;
                        default:
                            textBox.Text = textBox.Name;
                            break;
                    }
                }
                else if (field is PdfCheckBoxWidgetFieldWidget)
                {
                    PdfCheckBoxWidgetFieldWidget checkBoxField = field as PdfCheckBoxWidgetFieldWidget;
                    switch(checkBoxField.Name)
                    {
                        case "ST Strength":
                            checkBoxField.Checked = character.SavingThrows.HasFlag(AbilityTraining.Strength);
                            break;
                        case "ST Dexterity":
                            checkBoxField.Checked = character.SavingThrows.HasFlag(AbilityTraining.Dexterity);
                            break;
                        case "ST Constitution":
                            checkBoxField.Checked = character.SavingThrows.HasFlag(AbilityTraining.Constitution);
                            break;
                        case "ST Intelligence":
                            checkBoxField.Checked = character.SavingThrows.HasFlag(AbilityTraining.Intelligence);
                            break;
                        case "ST Wisdom":
                            checkBoxField.Checked = character.SavingThrows.HasFlag(AbilityTraining.Wisdom);
                            break;
                        case "ST Charisma":
                            checkBoxField.Checked = character.SavingThrows.HasFlag(AbilityTraining.Charisma);
                            break;
                        case "ChBx Athletics":
                            if (character.SkillExcellencies.HasFlag(SkillTraining.Athletics))
                            {
                                checkBoxField.BorderStyle = PdfBorderStyle.Dashed;
                                checkBoxField.BorderWidth = 2;
                            }
                            checkBoxField.Checked = character.SkillProfiencies.HasFlag(SkillTraining.Athletics);
                            break;
                        case "ChBx Acrobatics":
                            if (character.SkillExcellencies.HasFlag(SkillTraining.Acrobatics))
                            {
                                checkBoxField.BorderStyle = PdfBorderStyle.Dashed;
                                checkBoxField.BorderWidth = 2;
                            }
                            checkBoxField.Checked = character.SkillProfiencies.HasFlag(SkillTraining.Acrobatics);
                            break;
                        case "ChBx Sleight":
                            if (character.SkillExcellencies.HasFlag(SkillTraining.SlieghtOfHand))
                            {
                                checkBoxField.BorderStyle = PdfBorderStyle.Dashed;
                                checkBoxField.BorderWidth = 2;
                            }
                            checkBoxField.Checked = character.SkillProfiencies.HasFlag(SkillTraining.SlieghtOfHand);
                            break;
                        case "ChBx Stealth":
                            if (character.SkillExcellencies.HasFlag(SkillTraining.Stealth))
                            {
                                checkBoxField.BorderStyle = PdfBorderStyle.Dashed;
                                checkBoxField.BorderWidth = 2;
                            }
                            checkBoxField.Checked = character.SkillProfiencies.HasFlag(SkillTraining.Stealth);
                            break;
                        case "ChBx Arcana":
                            if (character.SkillExcellencies.HasFlag(SkillTraining.Arcana))
                            {
                                checkBoxField.BorderStyle = PdfBorderStyle.Dashed;
                                checkBoxField.BorderWidth = 2;
                            }
                            checkBoxField.Checked = character.SkillProfiencies.HasFlag(SkillTraining.Arcana);
                            break;
                        case "ChBx History":
                            if (character.SkillExcellencies.HasFlag(SkillTraining.History))
                            {
                                checkBoxField.BorderStyle = PdfBorderStyle.Dashed;
                                checkBoxField.BorderWidth = 2;
                            }
                            checkBoxField.Checked = character.SkillProfiencies.HasFlag(SkillTraining.History);
                            break;
                        case "ChBx Investigation":
                            if (character.SkillExcellencies.HasFlag(SkillTraining.Investigation))
                            {
                                checkBoxField.BorderStyle = PdfBorderStyle.Dashed;
                                checkBoxField.BorderWidth = 2;
                            }
                            checkBoxField.Checked = character.SkillProfiencies.HasFlag(SkillTraining.Investigation);
                            break;
                        case "ChBx Nature":
                            if (character.SkillExcellencies.HasFlag(SkillTraining.Nature))
                            {
                                checkBoxField.BorderStyle = PdfBorderStyle.Dashed;
                                checkBoxField.BorderWidth = 2;
                            }
                            checkBoxField.Checked = character.SkillProfiencies.HasFlag(SkillTraining.Nature);
                            break;
                        case "ChBx Religion":
                            if (character.SkillExcellencies.HasFlag(SkillTraining.Religion))
                            {
                                checkBoxField.BorderStyle = PdfBorderStyle.Dashed;
                                checkBoxField.BorderWidth = 2;
                            }
                            checkBoxField.Checked = character.SkillProfiencies.HasFlag(SkillTraining.Religion);
                            break;
                        case "ChBx Animal":
                            if (character.SkillExcellencies.HasFlag(SkillTraining.AnimalHandling))
                            {
                                checkBoxField.BorderStyle = PdfBorderStyle.Dashed;
                                checkBoxField.BorderWidth = 2;
                            }
                            checkBoxField.Checked = character.SkillProfiencies.HasFlag(SkillTraining.AnimalHandling);
                            break;
                        case "ChBx Insight":
                            if (character.SkillExcellencies.HasFlag(SkillTraining.Insight))
                            {
                                checkBoxField.BorderStyle = PdfBorderStyle.Dashed;
                                checkBoxField.BorderWidth = 2;
                            }
                            checkBoxField.Checked = character.SkillProfiencies.HasFlag(SkillTraining.Insight);
                            break;
                        case "ChBx Medicine":
                            if (character.SkillExcellencies.HasFlag(SkillTraining.Medicine))
                            {
                                checkBoxField.BorderStyle = PdfBorderStyle.Dashed;
                                checkBoxField.BorderWidth = 2;
                            }
                            checkBoxField.Checked = character.SkillProfiencies.HasFlag(SkillTraining.Medicine);
                            break;
                        case "ChBx Perception":
                            if (character.SkillExcellencies.HasFlag(SkillTraining.Perception))
                            {
                                checkBoxField.BorderStyle = PdfBorderStyle.Dashed;
                                checkBoxField.BorderWidth = 2;
                            }
                            checkBoxField.Checked = character.SkillProfiencies.HasFlag(SkillTraining.Perception);
                            break;
                        case "ChBx Survival":
                            if (character.SkillExcellencies.HasFlag(SkillTraining.Survival))
                            {
                                checkBoxField.BorderStyle = PdfBorderStyle.Dashed;
                                checkBoxField.BorderWidth = 2;
                            }
                            checkBoxField.Checked = character.SkillProfiencies.HasFlag(SkillTraining.Survival);
                            break;
                        case "ChBx Deception":
                            if (character.SkillExcellencies.HasFlag(SkillTraining.Deception))
                            {
                                checkBoxField.BorderStyle = PdfBorderStyle.Dashed;
                                checkBoxField.BorderWidth = 2;
                            }
                            checkBoxField.Checked = character.SkillProfiencies.HasFlag(SkillTraining.Deception);
                            break;
                        case "ChBx Intimidation":
                            if (character.SkillExcellencies.HasFlag(SkillTraining.Intimidation))
                            {
                                checkBoxField.BorderStyle = PdfBorderStyle.Dashed;
                                checkBoxField.BorderWidth = 2;
                            }
                            checkBoxField.Checked = character.SkillProfiencies.HasFlag(SkillTraining.Intimidation);
                            break;
                        case "ChBx Performance":
                            if (character.SkillExcellencies.HasFlag(SkillTraining.Performance))
                            {
                                checkBoxField.BorderStyle = PdfBorderStyle.Dashed;
                                checkBoxField.BorderWidth = 2;
                            }
                            checkBoxField.Checked = character.SkillProfiencies.HasFlag(SkillTraining.Performance);
                            break;
                        case "ChBx Persuasion":
                            if (character.SkillExcellencies.HasFlag(SkillTraining.Persuasion))
                            {
                                checkBoxField.BorderStyle = PdfBorderStyle.Dashed;
                                checkBoxField.BorderWidth = 2;
                            }
                            checkBoxField.Checked = character.SkillProfiencies.HasFlag(SkillTraining.Persuasion);
                            break;

                    }
                }
            }
            string fileType = charecterSheetLocation.Split('\\').Last();
            string file = Common.folderBase + character.Details.Name + "-Level " + character.CharecterLevel() + "-" + fileType;
            doc.SaveToFile(file);
            doc.Close();
            return file;
        }
    }
}
