using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;

namespace TestDatabaseCreator
{
    internal class TableMover : SmoObjectMover
    {
        private readonly ScriptingOptions tableOptions = new ScriptingOptions()
        {
            Indexes = false,
            ScriptDrops = false,
            IncludeIfNotExists = true,
            DriPrimaryKey = false
        };


        private HashSet<TableReference> moved;
        private HashSet<string> blacklist;

        public TableMover(SqlConnection Connection, string FromDatabase, string ToDatabase) :
            base(Connection, FromDatabase, ToDatabase) {
            moved = new HashSet<TableReference>();
        }

        public void Move(string objectName) {
            Move(
                new TableReference() {
                    TableName = objectName,
                    ColumnName = null,
                    ReferenceColumnName = null
                }, null);
        }

        public IEnumerable<string> Move(string objectName, Guid? pkValue, IEnumerable<string> Blacklist) {
            blacklist = new HashSet<string>(Blacklist);
            Move(
                new TableReference()
                {
                    TableName = objectName,
                    ColumnName = null,
                    ReferenceColumnName = null
                }, pkValue);

            return moved.Select(x => x.TableName).Distinct();
        }
       

        private void Move(TableReference t, Guid? primaryKey)
        {
            if (blacklist.Contains(t.TableName)) {
                return;
            }

            Debug.WriteLine(t.ReferenceTableName + "->" + t.TableName);
            moved.Add(t);

            var script = GetScript(t.TableName);

            RunSQLCollection(script, to);
            TransferData(t, primaryKey);
            foreach (var r in GetReferences(t.TableName))
            {
                if (!moved.Contains(r)) {
                    Move(r, null);
                }
            }
        }

        private StringCollection GetScript(string tableName) {
            return smoServer.Databases[from].Tables[tableName].Script(tableOptions);
        }

        private void TransferData(TableReference tref, Guid? primaryKey)
        {
            string sql;

            var cols = string.Join(", ", GetColumnList(tref.TableName));

            if (HasIdentityColumn(tref.TableName)) {
                SetIdentityInsertOn(tref.TableName);
            }

            var pkCol = GetPrimaryKeyColumn(tref.TableName);

            if (tref.ColumnName != null)
            {
                sql = string.Format(@"
		        insert into {1}..{2} with(tablock) ({6})
		        select {6}
		        from {0}..{2} a
		        where (
                    exists(
			            select 1 
			            from {1}..{4} b
			            where b.{5} = a.{3}
                    ) --or a.{3} is null
                )
		", from, to, tref.TableName, tref.ColumnName, tref.ReferenceTableName, tref.ReferenceColumnName, cols);

            }
            else if (primaryKey.HasValue) {
                sql = string.Format(@"
		        insert into {1}..{2} with(tablock) ({3})
		        select {3}
		        from {0}..{2} a
                where {4} = '{5}'
                ", from, to, tref.TableName, cols, pkCol, primaryKey.Value);
            }
            else {
                sql = string.Format(@"
		        insert into {1}..{2} with(tablock) ({3})
		        select {3}
		        from {0}..{2} a
                ", from, to, tref.TableName, cols);
            }


            if (pkCol != null)
            {
                sql += string.Format(@"
                and not exists(
                    select 1
                    from {0}..{1} _a
                    where _a.{2} = a.{2}
                )
        ", to, tref.TableName, pkCol);
            }

            RunSQL(sql, to);

            if (HasIdentityColumn(tref.TableName))
            {
                SetIdentityInsertOff(tref.TableName);
            }

        }

        private string GetPrimaryKeyColumn(string p) {
            var pkSQL = string.Format(@"
                use {0};

		        select top 1 c.name 
				from sys.indexes i
					inner join sys.index_columns ic
						on i.object_id = ic.object_id
					inner join sys.columns c
						on c.object_id = i.object_id
						and c.column_id = ic.column_id 
				where i.is_primary_key = 1
				and i.object_id = object_id('{1}');
            ", from, p);

            var cmd = new SqlCommand(pkSQL, sql);
           
            return (string) cmd.ExecuteScalar();
        }

        private IEnumerable<string> GetColumnList(string tableName) {
            string colSQL = string.Format(@"
                select name from sys.columns where object_id = object_id('{0}') and is_computed = 0;
            ", tableName);

            sql.ChangeDatabase(from);
            var cols = new List<string>();
            var cmd = new SqlCommand(colSQL, sql);

            using (var r = cmd.ExecuteReader())
            {
                while (r.Read()) {
                    cols.Add(r.GetString(0));
                }
            }

            return cols;
        }

        private void SetIdentityInsertOff(string tbl) {
            RunSQL(string.Format("SET IDENTITY_INSERT {0} OFF", tbl), to);
        }

        private void SetIdentityInsertOn(string tbl)
        {
            RunSQL(string.Format("SET IDENTITY_INSERT {0} ON", tbl), to);
        }

        private bool HasIdentityColumn(string tableName)
        {
            string colSQL = string.Format(@"
                select name from sys.columns where object_id = object_id('{0}') and is_identity = 1;
            ", tableName);

            sql.ChangeDatabase(from);
            var cmd = new SqlCommand(colSQL, sql);

            var s = (string)cmd.ExecuteScalar();
            return (s != null);
        }


        private IEnumerable<TableReference> GetReferences(string table)
        {
            string refSql = string.Format(@"
	select object_name(fk.parent_object_id), object_name(fk.referenced_object_id), tc.name, rc.name, 0
	from sys.foreign_keys fk
		inner join sys.foreign_key_columns fkc
			on fk.object_id = fkc.constraint_object_id
		inner join sys.columns rc
			on fkc.referenced_object_id = rc.object_id
			and fkc.referenced_column_id = rc.column_id
		inner join sys.columns tc
			on fkc.parent_object_id = tc.object_id
			and fkc.parent_column_id = tc.column_id
	where fk.referenced_object_id = object_id('{0}')
", table);
            sql.ChangeDatabase(from);
            var refs = new List<TableReference>();
            var cmd = new SqlCommand(refSql, sql);

            using (var r = cmd.ExecuteReader())
            {
                while (r.Read()) {
                    refs.Add(new TableReference() {
                        TableName = r.GetString(0),
                        ReferenceTableName = r.GetString(1),
                        ColumnName = r.GetString(2),
                        ReferenceColumnName = r.GetString(3)
                    });
                }
            }

            return refs.Where(x => x.TableName != x.ReferenceTableName);
        }

    }

    internal struct TableReference
    {
        public string TableName { get; set; }
        public string ColumnName { get; set; }
        public string ReferenceTableName { get; set; }
        public string ReferenceColumnName { get; set; }
    }

}
