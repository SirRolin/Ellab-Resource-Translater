using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellab_Resource_Translater.Objects
{
    internal interface IDBparameterable
    {
        DbParameter CreateParameter();
        DbParameterCollection Parameters { get; }
    }
    internal class DBCommandWrapper(DbCommand command) : IDBparameterable
    {
        public DbParameterCollection Parameters => command.Parameters;

        public DbParameter CreateParameter()
        {
            return command.CreateParameter();
        }
    }
    internal class DBBatchCommandWrapper(DbBatchCommand command) : IDBparameterable
    {
        public DbParameterCollection Parameters => command.Parameters;

        public DbParameter CreateParameter()
        {
            return command.CreateParameter();
        }
    }
}
