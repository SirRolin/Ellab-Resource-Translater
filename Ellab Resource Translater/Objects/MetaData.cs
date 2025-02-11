using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellab_Resource_Translater.Objects
{
    internal class MetaData<Type>(string key, Type value, string comment)
    {
        public string key = key;
        public Type value = value;
        public string comment = comment;

        override
        public string ToString()
        {
            return string.Concat(key, ": ", value?.ToString() ?? "null", " - ", comment);
        }
    }
}
