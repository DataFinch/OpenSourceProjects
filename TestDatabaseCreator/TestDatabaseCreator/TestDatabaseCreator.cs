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
        private List<string> tables;
        public string BackupPath { get; set; }

        public TestDatabaseCreator() {
            tables = new List<string>();
        }

        public void Create()
        {
            InitializeConnection();

            CreateDatabase();

            CreateTables();

            CreateIndexes();

            CreateForeignKeys();

            CreateViews();

            CreateStoredProcedures();

            //BackupDatabase();

            //Drop();
        }

        private void InitializeConnection() {
            sql = new SqlConnection(ConnectionString);
            sql.Open();
        }



        private void CreateDatabase() {
            var db = new DatabaseCreator(sql);
            db.Create(DatabaseName, TestDatabaseName);
        }

        
        private void CreateTables() {
            var m = new TableMover(sql, DatabaseName, TestDatabaseName, BlacklistedTables);

            foreach (var w in WhitelistedTables) {
                if (w.PrimaryKeyValue.HasValue) {
                    m.AddRootTable(w.Name, w.PrimaryKeyValue.Value);
                }
                else {
                    m.AddRootTable(w.Name);
                }
            }

            tables.AddRange(m.Move());
        }

        private void CreateIndexes() {
            var m = new IndexMover(sql, DatabaseName, TestDatabaseName);
            foreach (var t in tables) {
                m.Move(t);
            }
        }

        private void CreateForeignKeys()
        {
            var m = new ForeignKeyMover(sql, DatabaseName, TestDatabaseName);
            foreach (var t in tables)
            {
                m.Move(t);
            }
        }

        private void CreateViews() {
            var i = new ViewMover(sql, DatabaseName, TestDatabaseName);
            i.Move(TestDatabaseName);
        }

        private void CreateStoredProcedures() {
            var p = new StoredProcedureMover(sql, DatabaseName, TestDatabaseName);
            p.Move(TestDatabaseName);
        }

        private void BackupDatabase() {
            var b = new BackupCreator(sql, DatabaseName, TestDatabaseName);
            b.Backup(BackupPath);
        }

        private void Drop() {
            var d = new DatabaseDropper(sql, DatabaseName, TestDatabaseName);
            d.Drop();
        }
    }
}
