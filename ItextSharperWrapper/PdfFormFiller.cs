using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iTextSharp;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.xml;
using System.Collections;
using System.IO;

namespace ItextSharperWrapper
{
    public class FormItem
    {
        public string Name { get; set; }
        public string YesChechboxValue { get; set; }
        public bool IsCheckBox { get; set; }
    }

    public class PdfFormFiller
    {
        private string _fileName { get; set; }
        public List<FormItem> Fields { get; set; }

        public PdfFormFiller(string fileName)
        {
            _fileName = fileName;
            LoadFieldNames();

        }

        public void FillForm(string saveName, Dictionary<string, string> textField, Dictionary<string, bool> checkBoxes)
        {
            PdfReader reader = new PdfReader(this._fileName);
            PdfStamper writer = new PdfStamper(reader, new FileStream(saveName, FileMode.Create));
            foreach(FormItem field in Fields)
            {
                string trimName = field.Name.Trim();
                if(textField.ContainsKey(trimName) && !string.IsNullOrWhiteSpace(textField[trimName]))
                {
                    string value = textField[trimName];
                    writer.AcroFields.SetField(field.Name, value);
                }
                else if(checkBoxes.ContainsKey(trimName) && checkBoxes[trimName] && field.IsCheckBox)
                {
                    writer.AcroFields.SetField(field.Name, field.YesChechboxValue);
                }
            }
            writer.FormFlattening = false;
            writer.Close();
        }

        private void LoadFieldNames()
        {
            Fields = new List<FormItem>();
            using(PdfReader reader = new PdfReader(_fileName))
            {
                Fields.AddRange(
                    reader.AcroForm.Fields.Select(
                        field => 
                        {
                            FormItem item = new FormItem();
                            item.Name = field.Name;
                            item.IsCheckBox = reader.AcroFields.GetFieldType(field.Name) == AcroFields.FIELD_TYPE_CHECKBOX;
                            if(item.IsCheckBox)
                            {
                                var apperanceStates = reader.AcroFields.GetAppearanceStates(item.Name);
                                item.YesChechboxValue = apperanceStates[0];
                            }
                            return item;
                        })
                    );
            }
            
        }

    }
}
