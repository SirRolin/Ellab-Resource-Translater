using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellab_Resource_Translater.objects
{
    internal class Translation(string key, string value, string comment)
    {
        public string key = key;
        public string value = value;
        public string comment = comment;

        override
        public string ToString()
        {
            return string.Concat(key, ": ", value, " - ", comment);
        }
    }
}
