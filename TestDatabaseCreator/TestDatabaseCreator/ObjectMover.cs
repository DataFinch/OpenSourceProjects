using System;
using System.Collections.Generic;
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

        public abstract void Move(string objectName);
    }
}
