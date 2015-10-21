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

        private HashSet<string> blacklist;
        private Dictionary<string, Table> Tables;

        public TableMover(SqlConnection Connection, string FromDatabase, string ToDatabase, IEnumerable<string> Blacklist) :
            base(Connection, FromDatabase, ToDatabase) {
            Tables = new Dictionary<string, Table>();
            blacklist = new HashSet<string>(Blacklist);
        }

        public void AddRootTable(string name) {
            if (!Tables.ContainsKey(name)) {
                Tables.Add(name, new Table(name));
            }
        }

        public void AddRootTable(string name, Guid pkValue) {
            AddRootTable(name);
            Tables[name].PKValue = pkValue;
        }

        public IEnumerable<string> Move() {
            //build map
            var roots = new List<Table>(Tables.Values);
            foreach(var r in roots) {
                Map(r);
            }

            //create tables
            var tbls = new List<Table>(Tables.Values);
            foreach (var t in tbls) {
                var script = GetScript(t.Name);
                RunSQLCollection(script, to);
            }

            //move data
            while (tbls.Any()) {
                var movable = tbls.Where(x => x.CanMove).ToList();
                var t = tbls.Where(x => x.CanMove).First();
                if (!blacklist.Contains(t.Name)) {
                    TransferData(t, t.PKValue);
                }
                t.Moved();
                tbls.Remove(t);
            }

            return Tables.Keys;
        }
       

        private void Map(Table t)
        {
            if (!Tables.ContainsKey(t.Name)) {
                Tables.Add(t.Name, t);
            }

            foreach (var r in GetReferences(t.Name))
            {
                Map(Tables[r]);
            }
        }

        private StringCollection GetScript(string tableName) {
            return smoServer.Databases[from].Tables[tableName].Script(tableOptions);
        }

        private void TransferData(Table t, Guid? primaryKey)
        {
            StringBuilder sql = new StringBuilder();

            var cols = string.Join(", ", GetColumnList(t.Name));

            if (HasIdentityColumn(t.Name)) {
                SetIdentityInsertOn(t.Name);
            }

            var pkCol = GetPrimaryKeyColumn(t.Name);

            sql.AppendLine(string.Format("insert into [{0}]..[{1}] with(tablock) ({2})", to, t.Name, cols));
            sql.AppendLine(string.Format("select {0}", cols));
            sql.AppendLine(string.Format("from [{0}]..[{1}] a", from, t.Name));
            sql.AppendLine("where 1=1");

            if (primaryKey.HasValue) {
                sql.AppendLine(string.Format("and {0} = '{1}'", pkCol, primaryKey.Value));
            }

            if (t.References.Any()) {
                bool first = true;
                sql.AppendLine("and (");
                foreach(var r in t.References) {
                    if (!first) {
                        sql.AppendLine("OR");
                    }
                    sql.AppendLine("exists(");
                    sql.AppendLine("select 1");
                    sql.AppendLine(string.Format("from [{0}]..[{1}] _a", to, r.ForeignTable.Name));
                    sql.AppendLine(string.Format("where _a.{0} = a.{1}", r.ForeignColumnName, r.ColumnName));
                    sql.AppendLine(")");
                } 
                sql.AppendLine(")");
            }

            if(pkCol != null) {
                sql.AppendLine("and not exists(");
                sql.AppendLine("select 1");
                sql.AppendLine(string.Format("from [{0}]..[{1}] _a", to, t.Name));
                sql.AppendLine(string.Format("where _a.{0} = a.{0}", pkCol));
                sql.AppendLine(")");
            }


            RunSQL(sql.ToString(), to);

            if (HasIdentityColumn(t.Name))
            {
                SetIdentityInsertOff(t.Name);
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


        private IEnumerable<string> GetReferences(string table)
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
            var refs = new List<string>();
            var cmd = new SqlCommand(refSql, sql);

            using (var r = cmd.ExecuteReader())
            {
                while (r.Read()) {
                    var name = r.GetString(0);
                    if (name != table) {
                        if (!Tables.ContainsKey(name)) {
                            Tables.Add(name, new Table(name));
                            refs.Add(name);
                            Tables[name].AddReference(
                                new Reference() {
                                    ForeignTable = Tables[table],
                                    ColumnName = r.GetString(2),
                                    ForeignColumnName = r.GetString(3)
                                });
                        }
                    }
                }
            }
            return refs;
        }

    }

    internal class Table {
        public string Name;
        public Guid? PKValue { get; set; }

        public List<Reference> References { get; private set; }
        private bool IsMoved;

        public Table(string Name) {
            References = new List<Reference>();
            IsMoved = false;
            this.Name = Name;
        }

        public void AddReference(Reference r) {
            References.Add(r);
        }

        public bool CanMove {
            get {
                return References.All(x => x.ForeignTable.IsMoved);
            }
        }

        public void Moved() {
            IsMoved = true;
        }
    }

    internal struct Reference
    {
        public string ColumnName { get; set; }
        public Table ForeignTable { get; set; }
        public string ForeignColumnName { get; set; }
    }

}
