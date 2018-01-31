using System;

namespace Query
{
    internal class TableNameAttribute : Attribute
    {
        public string TableName { get; private set; }
        public TableNameAttribute(string name)
            => TableName = name;
    }
}