using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Ellab_Resource_Translater.Objects.Extensions
{
    static class MetaDataExt
    {
        public static ResXDataNode? ToResXDataNode(this MetaData<object?> meta)
        {
            if (meta.value is ISerializable iSer)
                return new(meta.key, iSer)
                {
                    Comment = meta.comment
                };
            return null;
        }

        public static void WriteToResourceWriter(this MetaData<object?> meta, ResXResourceWriter writer)
        {
            if (meta.value is ISerializable iSer)
            {
                writer.AddResource(new(meta.key, iSer)
                {
                    Comment = meta.comment
                });
            } 
            // Cause Apparently strings are not ISerializable. Though they can be serialized.
            else if (meta.value is string iStr)
            {
                writer.AddResource(new(meta.key, iStr)
                {
                    Comment = meta.comment
                });
            }
        }
    }
}
