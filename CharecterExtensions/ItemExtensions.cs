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
    public static class ItemExtensions
    {
        public static string DefaultItemData
        {
            get
            {
                string data;
                try
                {
                    Assembly assem = Assembly.GetExecutingAssembly();
                    var _textStreamReader = new StreamReader(assem.GetManifestResourceStream("CharecterExtensions.Item"));
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

        public static DataHandler<Item> CharecterProvider = new DataHandler<Item>(Common.folderBase, DefaultItemData);

        static ItemExtensions()
        {
        }

        public static List<Item> LoadAll(this List<Item> characters)
        {
            characters = CharecterProvider.GetAll();
            return characters;
        }

        public static Item Load(this Item characterToLoad)
        {
            characterToLoad = characterToLoad.Load(characterToLoad.Id);
            return characterToLoad;
        }

        public static Item Load(this Item characterToLoad, int idToPopulate)
        {
            characterToLoad = CharecterProvider.GetById(idToPopulate);
            return characterToLoad;
        }

        public static Item Save(this Item characterToSave)
        {
            CharecterProvider.Save(characterToSave);
            return characterToSave;
        }

        public static void Delete(this Item character)
        {
            CharecterProvider.Delete(character.Id);
        }
        public static void Delete(this Item character, int idToDelete)
        {
            CharecterProvider.Delete(idToDelete);
        }

        public static string AttackModifier(this Item item, Character charecterToUse, bool isForAttack = true)
        {
            int dex = charecterToUse.Abilities.Dexterity.Modifier();
            int str = charecterToUse.Abilities.Strength.Modifier();
            int modToUse;
            string display;
            bool isThrown = item.Thrown;
            bool isFinesse = item.Finesse;
            bool isRanged = !string.IsNullOrWhiteSpace(item.Range);
            if(isThrown || isFinesse)
            {
                if(str > dex)
                {
                    modToUse = str;
                }
                else
                {
                    modToUse = dex;
                }
            }
            else
            {
                if(isRanged)
                {
                    modToUse = dex;
                }
                else
                {
                    modToUse = str;
                }
            }
            if(isForAttack)
            {
                if (charecterToUse.SpecialOverides.ProfiencyBonus != 0)
                {
                    modToUse += charecterToUse.SpecialOverides.ProfiencyBonus;
                }
                else
                {
                    modToUse += charecterToUse.ProfiencyBonus();
                }
            }
            if (isForAttack)
            {
                display = modToUse.ToString("+#;-#;+0");
            }
            else
            {
                if(modToUse == 0)
                {
                    display = string.Empty;
                }
                else
                {
                    display = modToUse.ToString("+#;-#;+0");
                }
            }

            return display;
        }
    }
}
