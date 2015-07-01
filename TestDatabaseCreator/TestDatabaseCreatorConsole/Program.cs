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
                WhitelistedTables = new List<string> {  "Organization" }
            };
            creator.Create();
        }
    }
}
