using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellab_Resource_Translater.Structs
{
    public readonly struct Indexed<T>(T item, int index)
    {
        public readonly int Index => index;
        public readonly T Item => item;
    }
}
