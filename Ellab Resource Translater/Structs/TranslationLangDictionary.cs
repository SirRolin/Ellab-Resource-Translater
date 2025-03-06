using Ellab_Resource_Translater.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellab_Resource_Translater.Structs
{
    public struct TranslationLangDictionary<T>(Dictionary<string, Dictionary<string, MetaData<T>>> dict)
    {
        public readonly Dictionary<string, Dictionary<string, MetaData<T>>> Dict => dict;
    }
}
