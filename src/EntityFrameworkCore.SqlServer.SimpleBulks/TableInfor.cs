namespace EntityFrameworkCore.SqlServer.SimpleBulks
{
    public class TableInfor
    {
        public string Schema { get; private set; }

        public string Name { get; private set; }

        public string SchemaQualifiedTableName { get; private set; }

        public TableInfor(string schema, string tableName)
        {
            Schema = schema;
            Name = tableName;

            SchemaQualifiedTableName = string.IsNullOrEmpty(schema) ? $"[{tableName}]" : $"[{schema}].[{tableName}]";
        }

        public TableInfor(string tableName) : this(null, tableName)
        {
        }
    }
}
