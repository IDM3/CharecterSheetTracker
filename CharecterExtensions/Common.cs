using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CharecterExtensions
{
    public static class Common
    {
        public static string folderBase
        {
            get
            {
                string location = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CharecterSheet");
                if(!Directory.Exists(location))
                {
                    Directory.CreateDirectory(location);
                }
                return location.TrimEnd('\\') + "\\";
            }
        }
    }
}
