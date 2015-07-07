using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.Management.Smo;

namespace TestDatabaseCreator
{
    internal class IndexMover : SmoObjectMover
    {

        public IndexMover(SqlConnection Connection, string FromDatabase, string ToDatabase) :
            base(Connection, FromDatabase, ToDatabase) { }

        private ScriptingOptions options = new ScriptingOptions()
        {
            IncludeIfNotExists = true,
            NonClusteredIndexes = true,
            ClusteredIndexes = true,
            DriPrimaryKey = true
        };


        public override void Move(string objectName) {
            var indexes = smoServer.Databases[from].Tables[objectName].Indexes;

            for (int i = 0; i < indexes.Count; i++) {
                Debug.WriteLine(string.Format("Creating index {0} on {1}", indexes[i].Name, objectName));
                var script = CondenseStringCollection(indexes[i].Script(options));

                RunSQL(script, to);
            }    
        }
    }
}
