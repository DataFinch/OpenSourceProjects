using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.Management.Smo;

namespace TestDatabaseCreator
{
    internal class DatabaseDropper : SmoObjectMover
    {

        public DatabaseDropper(SqlConnection Connection, string FromDatabase, string ToDatabase) :
            base(Connection, FromDatabase, ToDatabase)
        {

        }

        public void Drop() {
            smoServer.Databases[to].Drop();
        }
    }
}
