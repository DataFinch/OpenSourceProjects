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
                ConnectionString = "Server=.\\sqlexpress;Database=master;Trusted_Connection=True;",
                DatabaseName = "TherapistPortal",
                TestDatabaseName = "TestCopy",
                WhitelistedTables = new List<WhiteListedTable> {
                    new WhiteListedTable() {
                        //Name = "Organization", PrimaryKeyValue = new Guid("68E13B96-5140-480E-9B8D-391ADC75AA20")
                        Name = "Organization", PrimaryKeyValue = new Guid("DC39ACF6-779C-4850-85F9-A542CAC8ED88")
                    }
                }
            };
            creator.Create();
        }
    }
}
