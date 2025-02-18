using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;


namespace Ellab_Resource_Translater.Objects
{
    public class MetaData<Type>(string key, Type value, string comment)
    {
        public string key = key;
        public Type value = value;
        public string comment = comment;

        override
        public string ToString()
        {
            return string.Concat(key, ": ", value?.ToString() ?? "null", " - ", comment);
        }

        public static Dictionary<string, MetaData<Type>> FilterTo(Dictionary<string, MetaData<object?>> metaData)
        {
            Dictionary<string, MetaData<Type>> output = [];
            foreach (var item in metaData)
            {
                if (item.Value.value is Type typed)
                {
                    output.Add(item.Key, new MetaData<Type>(item.Key, typed, item.Value.comment));
                }
            }
            return output;
        }
    }

    public static class MetaDataHelper
    {
        public static ResXDataNode? ToResXDataNode(this MetaData<object?> meta) {
            if(meta.value is ISerializable iSer)
                return new (meta.key, iSer) { 
                    Comment = meta.comment
                };
            return null;
        }

        public static void WriteToResourceWriter(this MetaData<object?> meta, ResXResourceWriter writer)
        {
            if (meta.value is ISerializable iSer)
                writer.AddResource(new(meta.key, iSer)
                {
                    Comment = meta.comment
                });
        }

    }
}
