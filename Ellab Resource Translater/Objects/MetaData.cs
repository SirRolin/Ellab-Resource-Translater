using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;


namespace Ellab_Resource_Translater.Objects
{
    public class MetaData<Type>(string key, Type value, string comment, string language = "EN")
    {
        public string key = key;
        public Type value = value;
        public string comment = comment;
        public string language = language;

        override
        public string ToString()
        {
            return string.Concat(language, "|", key, ": ", value?.ToString() ?? "null", " - ", comment);
        }

        public static Dictionary<string, MetaData<Type>> FilterTo(Dictionary<string, MetaData<object?>> metaData)
        {
            Dictionary<string, MetaData<Type>> output = [];
            foreach (var item in metaData)
            {
                if (item.Value.value is Type typed)
                {
                    output.Add(item.Key, new MetaData<Type>(item.Key, typed, item.Value.comment, item.Value.language));
                }
            }
            return output;
        }
    }
}
