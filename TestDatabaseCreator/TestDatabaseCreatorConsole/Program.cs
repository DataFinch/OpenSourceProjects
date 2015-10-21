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
            var org = new Guid("DC39ACF6-779C-4850-85F9-A542CAC8ED88");
            var creator = new TestDatabaseCreator.TestDatabaseCreator() {
                ConnectionString = "Server=192.168.58.55;Database=master;Trusted_Connection=True;",
                //ConnectionString = "Server=.\\sqlexpress;Database=master;Trusted_Connection=True;",
                DatabaseName = "TherapistPortal",
                TestDatabaseName = "DFT",
                WhitelistedTables = new List<WhiteListedTable> {
                    new WhiteListedTable() {
                        //Name = "Organization", PrimaryKeyValue = new Guid("68E13B96-5140-480E-9B8D-391ADC75AA20")
                        //Name = "Organization", PrimaryKeyValue = new Guid("DC39ACF6-779C-4850-85F9-A542CAC8ED88")
                        //Name = "Organization", PrimaryKeyValue = new Guid("3ADAC953-4634-4B27-990A-E6FEB02B0DDF")
                        Name = "Organization", PrimaryKeyValue = org
                    },
                    new WhiteListedTable() { Name = "Announcements" },
                    new WhiteListedTable() { Name = "DiagnosticCommands" },
                    new WhiteListedTable() { Name = "NumbersTest" },
                    new WhiteListedTable() { Name = "OutboundMessageQueue" },
                    new WhiteListedTable() { Name = "PromptSchedules" },
                    new WhiteListedTable() { Name = "Rights" },
                    new WhiteListedTable() { Name = "SavedReports" },
                    new WhiteListedTable() { Name = "SeminarRegistration" },
                    new WhiteListedTable() { Name = "SiteUserAssignments" },
                    new WhiteListedTable() { Name = "StudentCloneQueue" },
                    new WhiteListedTable() { Name = "TimezoneLookup" },
                    new WhiteListedTable() { Name = "ProgressReportItemProgress" },
                    new WhiteListedTable() { Name = "tools_calendar" },
                    new WhiteListedTable() { Name = "Widget_AverageTrialsPerDay" },
                    new WhiteListedTable() { Name = "Widget_LowestPerformingTargets" },
                    new WhiteListedTable() { Name = "WidgetBase" },
                    new WhiteListedTable() { Name = "OrganizationIntegrations" },
                    new WhiteListedTable() { Name = "SyncErrors" },
                    new WhiteListedTable() { Name = "schema_info" }

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
