using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellab_Resource_Translater.Objects
{
    /// <summary>
    /// Since the buildin <see cref="Delegate"/> copies variables without being able to keep a reference.
    /// We can use this to pass a referenced variable.
    /// </summary>
    /// <param name="value"></param>
    internal class Ref<T>(T value)
    {
        public T value = value;
        public static implicit operator T(Ref<T> intRef) => intRef.value;
        public static implicit operator Ref<T>(T val) => new(val);
    }
}
