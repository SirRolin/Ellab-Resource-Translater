using Microsoft.Data.SqlClient;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellab_Resource_Translater.Objects
{
    internal class DbConnectionExtension(DbConnection connection, string connectionString)
    {
        public DbConnection connection = connection;
        public bool canMultiResult = !connectionString.Contains("MultipleActiveResultSets=False");

        private static readonly SemaphoreSlim _semaphore = new(1, 1);

        public async Task ThreadSafeAsyncFunction(Action<DbConnection> query)
        {
            // In case we can have multiple Result Sets
            if (canMultiResult)
            {
                query.Invoke(connection);
                return;
            }

            // Threadsafe in case we can't
            await _semaphore.WaitAsync();
            try
            {
                query.Invoke(connection);
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
