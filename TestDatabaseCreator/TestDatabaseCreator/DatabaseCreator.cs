using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestDatabaseCreator
{
    internal class DatabaseCreator : ObjectMover
    {

        public DatabaseCreator(SqlConnection Connection)
            : base(Connection, null, null) {
        }

        public void Move(string objectName)
        {
            var drop = string.Format(@"
    if(exists(select 1 from sys.databases where name = '{0}'))
	begin
        alter database {0} set single_user with rollback immediate; 
        drop database {0};
    end", objectName);

            var create = string.Format("create database {0}", objectName);

            RunSQL(drop, "master");
            RunSQL(create, "master");
        }
    }
}
