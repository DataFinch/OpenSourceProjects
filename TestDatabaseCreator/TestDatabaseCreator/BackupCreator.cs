using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.Management.Smo;

namespace TestDatabaseCreator
{
    internal class BackupCreator : SmoObjectMover
    {

        public BackupCreator(SqlConnection Connection, string FromDatabase, string ToDatabase) :
            base(Connection, FromDatabase, ToDatabase) {

        }

        public void Backup(string BackupPath) {
            BackupDeviceItem d = new BackupDeviceItem(BackupPath, DeviceType.File);

            Backup b = new Backup() {
                Database = to,
                CompressionOption = BackupCompressionOptions.Default,
                Initialize = true,
                Action = BackupActionType.Database,
                Incremental = false
            };
            b.Devices.Add(d);
            b.SqlBackup(smoServer);
        }
    }
}
