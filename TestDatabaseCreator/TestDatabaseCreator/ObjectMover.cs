using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestDatabaseCreator
{
    internal abstract class ObjectMover {

        protected string from;
        protected string to;
        protected SqlConnection sql;

        public ObjectMover(SqlConnection Connection, string FromDatabase, string ToDatabase) {
            from = FromDatabase;
            to = ToDatabase;
            sql = Connection;
        }

        protected void RunSQL(string command, string database) {
            sql.ChangeDatabase(database);
            using (var cmd = new SqlCommand(command, sql)) {
                cmd.ExecuteNonQuery();
            }
        }

        protected void RunSQLCollection(StringCollection commands, string database) {
            foreach (var c in commands) {
                RunSQL(c, database);
            }

        }
    }
}
