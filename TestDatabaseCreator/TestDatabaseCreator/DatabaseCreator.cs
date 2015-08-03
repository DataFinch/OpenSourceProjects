using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestDatabaseCreator
{
    internal class DatabaseCreator : SmoObjectMover
    {

        public DatabaseCreator(SqlConnection Connection)
            : base(Connection, null, null) {
        }

        public void Create(string sourceDatabaseName, string newDatabaseName) {
            var path = smoServer.Databases[sourceDatabaseName].PrimaryFilePath;


            var drop = string.Format(@"
    if(exists(select 1 from sys.databases where name = '{0}'))
	begin
        alter database {0} set single_user with rollback immediate; 
        drop database {0};
    end", newDatabaseName);

            var create = string.Format(@"
                create database {0} on (
                    name = {0}_1, filename='{1}\{0}.mdf', filegrowth=10%
                )

            ", newDatabaseName, path);

            RunSQL(drop, "master");
            RunSQL(create, "master");
        }
    }
}
