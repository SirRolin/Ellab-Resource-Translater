using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellab_Resource_Translater.Enums
{
    public enum ConnType
    {
        None = 0,
        MySql = 1 << 0,
        MSSql = 1 << 1,
        PostgreSql = 1 << 2
    }
}
