using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharecterExtensions
{
    public static class MiscExtensions
    {
        public static int Modifier(this int stat)
        {
            int modifier = stat;
            if(stat%2 != 0)
            {
                modifier--;
            }
            else
            {
                //is even
            }
            modifier -= 10;
            modifier /= 2;
            return modifier;
        }
    }
}
