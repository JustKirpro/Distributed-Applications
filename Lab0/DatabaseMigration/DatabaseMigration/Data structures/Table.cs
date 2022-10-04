using System.Collections.Generic;

namespace DatabaseMigration.Data_structures
{
    public struct Table
    {
        public string Name { get; init; }
        public List<Dictionary<string, object>> Records { get; init; }
    }
}