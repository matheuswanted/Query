using System;

namespace Query
{
    internal class TableColumnAttribute : Attribute
    {
        public TableColumnAttribute(string column)
            => ColumnName = column;

        public string ColumnName { get; }
    }
}