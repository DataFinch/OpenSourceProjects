﻿using System;
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
        private HashSet<string> moved;

        public IndexMover(SqlConnection Connection, string FromDatabase, string ToDatabase) :
            base(Connection, FromDatabase, ToDatabase) {
            moved = new HashSet<string>();
        }

        private ScriptingOptions options = new ScriptingOptions()
        {
            IncludeIfNotExists = true,
            NonClusteredIndexes = true,
            ClusteredIndexes = true,
            DriPrimaryKey = true
        };


        public void Move(string objectName) {
            if (!moved.Contains(objectName)) {
                var indexes = smoServer.Databases[from].Tables[objectName].Indexes;

                for (int i = 0; i < indexes.Count; i++) {

                    Debug.WriteLine(string.Format("Creating index {0} on {1}", indexes[i].Name, objectName));
                    var script = indexes[i].Script(options);

                    RunSQLCollection(script, to);
                }
                moved.Add(objectName);
            }    
        }
    }
}
