using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;


namespace TestDatabaseCreator
{

    internal abstract class SmoObjectMover : ObjectMover
    {
        protected Server smoServer;

        public SmoObjectMover(SqlConnection Connection, string FromDatabase, string ToDatabase)
            :base(Connection, FromDatabase, ToDatabase){

                smoServer = new Server(new ServerConnection(Connection));

        }

      
    }
}
