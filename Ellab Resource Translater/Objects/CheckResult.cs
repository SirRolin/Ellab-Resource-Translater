using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellab_Resource_Translater.objects
{
    internal class CheckResult
    {
        public bool check;
        public string value;
        public CheckResult(bool check, string value)
        {
            this.check = check;
            this.value = value;
        }
    }
}
