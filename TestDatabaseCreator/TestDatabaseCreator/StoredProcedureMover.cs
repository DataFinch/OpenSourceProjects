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
    internal class StoredProcedureMover : SmoObjectMover
    {

        public StoredProcedureMover(SqlConnection Connection, string FromDatabase, string ToDatabase) :
            base(Connection, FromDatabase, ToDatabase)
        {

        }
        private ScriptingOptions so = new ScriptingOptions()
        {
            IncludeIfNotExists = true
        };

        public void Move(string objectName) {
            var procs = smoServer.Databases[from].StoredProcedures;

            for (int i = 0; i < procs.Count; i++)
            {
                var sp = procs[i];

                try
                {
                    if (!sp.IsSystemObject)
                    {
                        Debug.WriteLine(string.Format("Creating procedure {0}", sp.Name));
                        var script = sp.Script(so);
                        RunSQLCollection(script, to);
                    }
                }
                catch (SqlException ex)
                {
                    Debug.WriteLine(string.Format("Error creating procedure {0}", sp.Name));
                    Debug.WriteLine(ex.Message);
                }
            }
        }
    }
}