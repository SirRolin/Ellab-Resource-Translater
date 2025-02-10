using Ellab_Resource_Translater.Objects;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellab_Resource_Translater.Util
{
    internal class ConnectionProvider(string connectionString) : IDisposable
    {
        private List<DbConnectionExtension> dces = [];

        public void Dispose()
        {
            // Get rid of all active connections
            while (dces.Count > 0)
            {
                dces[0].Dispose();
            }
        }

        public DbConnectionExtension Get()
        {
            DbConnectionExtension dce = new(DBStringHandler.CreateDbConnection(connectionString));
            dces.Add(dce);
            dce.conn.Disposed += (s,e) => dces.Remove(dce);
            return dce;
        }
    }
}
