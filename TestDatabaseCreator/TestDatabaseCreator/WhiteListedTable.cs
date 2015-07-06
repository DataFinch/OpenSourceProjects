using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestDatabaseCreator
{
    public class WhiteListedTable
    {
        public string Name { get; set; }
        public Guid? PrimaryKeyValue { get; set; }
    }
}
