using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestDatabaseCreator
{
    public class TestDatabaseCreator
    {
        public bool IncludeData { get; set; }
        public bool IncludeViews { get; set; }
        public bool IncludeProgrammables { get; set; }
        public bool IncludeIndexes { get; set; }
        public string TestDatabaseName { get; set; }
        public string DatabaseName { get; set; }
        public string ConnectionString { get; set; }

        public IEnumerable<string> BlacklistedTables { get; set; }
        public IEnumerable<WhiteListedTable> WhitelistedTables { get; set; }

        private SqlConnection sql;

        private ScriptingOptions indexOptions = new ScriptingOptions() {
            IncludeIfNotExists = true,
            NonClusteredIndexes = true,
            ClusteredIndexes = true,
            DriAllConstraints = true
        };


        public TestDatabaseCreator()
        {
        }

        public void Create()
        {
            InitializeConnection();

            CreateDatabase();

            CreateTables();

            //indexes 

            //views

            //procs/functions

            //backup to file

            //drop
        }

        private void InitializeConnection() {
            sql = new SqlConnection(ConnectionString);
            sql.Open();
        }



        private void CreateDatabase() {
            var db = new DatabaseCreator(sql);
            db.Move(TestDatabaseName);
        }

        
        private void CreateTables() {
            var m = new TableMover(sql, DatabaseName, TestDatabaseName);

            foreach (var w in WhitelistedTables) {
                m.Move(w.Name, w.PrimaryKeyValue);
            }
        }
    }
}
