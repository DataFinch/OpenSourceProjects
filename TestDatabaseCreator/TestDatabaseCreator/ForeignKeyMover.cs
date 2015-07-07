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
    internal class ForeignKeyMover : SmoObjectMover
    {
        private HashSet<string> createdTables;

        public ForeignKeyMover(SqlConnection Connection, string FromDatabase, string ToDatabase) :
            base(Connection, FromDatabase, ToDatabase) {
            createdTables = new HashSet<string>();
            var tbls = smoServer.Databases[ToDatabase].Tables;
            for (int i = 0; i < tbls.Count; i++) {
                createdTables.Add(tbls[i].Name);
            }
        }

        private ScriptingOptions options = new ScriptingOptions()
        {
            IncludeIfNotExists = true,
            DriWithNoCheck = true //TODO: Remove this (and clean up the data)
        };


        public override void Move(string objectName) {
            var fks = smoServer.Databases[from].Tables[objectName].ForeignKeys;

            for (int i = 0; i < fks.Count; i++) {
                if (CanCreate(fks[i])) {
                    Debug.WriteLine(string.Format("Creating Foreign Key {0} on {1}", fks[i], objectName));
                    var script = CondenseStringCollection(fks[i].Script(options));
                    RunSQL(script, to);
                }
            }    
        }

        private bool CanCreate(ForeignKey k) {
            if (createdTables.Contains(k.ReferencedTable)) {
                return true;
            }
            return false;
        }
        
    }
}
