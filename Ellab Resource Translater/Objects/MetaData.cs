﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
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

        public static implicit operator ResXDataNode(MetaData<Type> meta) => new(meta.key, meta.value) { Comment = meta.comment };
    }
}
