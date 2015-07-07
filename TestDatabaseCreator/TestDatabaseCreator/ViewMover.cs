using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.Management.Smo;

namespace TestDatabaseCreator {
    internal class ViewMover : SmoObjectMover {

        public ViewMover(SqlConnection Connection, string FromDatabase, string ToDatabase) :
            base(Connection, FromDatabase, ToDatabase)
        {

        }
        private ScriptingOptions so = new ScriptingOptions() {
            IncludeIfNotExists = true
        };

        public void Move(string objectName) {
            var views = smoServer.Databases[from].Views;

            for (int i = 0; i < views.Count; i++) {
                var v = views[i];

                try {
                    if (!v.IsSystemObject) {
                        Debug.WriteLine(string.Format("Creating view {0}", v.Name));
                        var script = v.Script(so);
                        RunSQLCollection(script, to);
                    }
                }
                catch (SqlException ex) {
                    Debug.WriteLine(string.Format("Error creating view {0}", v.Name));
                    Debug.WriteLine(ex.Message);
                }
            }
        }
    }
}