using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace Ellab_Resource_Translater.Objects
{
    internal class AzureCredentials(string key, string uRI, string region)
    {
        public string Key { get; set; } = key;
        public string URI { get; set; } = uRI;
        public string Region { get; set; } = region;
    }
}
