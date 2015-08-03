using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestDatabaseCreator;

namespace TestDatabaseCreatorConsole
{
    public class Program
    {
        static void Main(string[] args) {
            var creator = new TestDatabaseCreator.TestDatabaseCreator() {
                ConnectionString = "Server=192.168.58.55;Database=master;Trusted_Connection=True;",
                DatabaseName = "TherapistPortal",
                TestDatabaseName = "DFT",
                WhitelistedTables = new List<WhiteListedTable> {
                    new WhiteListedTable() {
                        //Name = "Organization", PrimaryKeyValue = new Guid("68E13B96-5140-480E-9B8D-391ADC75AA20")
                        //Name = "Organization", PrimaryKeyValue = new Guid("DC39ACF6-779C-4850-85F9-A542CAC8ED88")
                        //Name = "Organization", PrimaryKeyValue = new Guid("3ADAC953-4634-4B27-990A-E6FEB02B0DDF")
                        Name = "Organization", PrimaryKeyValue = new Guid("DC39ACF6-779C-4850-85F9-A542CAC8ED88")
                    }
                },
                BackupPath = "C:\\Temp\\DFT.bak",
                BlacklistedTables = new List<string> {
                    "ProgressReportHistory"
                }

            };
            creator.Create();
        }
    }
}
