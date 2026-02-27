using System;

namespace EntityFrameworkCore.SqlServer.SimpleBulks;

public class BulkOptions
{
    public int BatchSize { get; set; }

    public int Timeout { get; set; }

    public Action<string> LogTo { get; set; }

    public string MappingProfileName { get; set; }

    public BulkOptions()
    {
        Timeout = 30;
    }
}

public record struct SetClauseContext
{
    public TableInfor TableInfor { get; set; }

    public string PropertyName { get; set; }

    public string Left { get; set; }

    public string Right { get; set; }

    public string TargetTableAlias { get; set; }

    public string SourceTableAlias { get; set; }

    private string Quote(string name)
    {
        return $"{Constants.BeginQuote}{name}{Constants.EndQuote}";
    }

    public string GetTargetTableColumn(string propertyName)
    {
        var columnName = TableInfor.GetDbColumnName(propertyName);

        return string.IsNullOrEmpty(TargetTableAlias) ? Quote(columnName) : $"{TargetTableAlias}.{Quote(columnName)}";
    }

    public string GetSourceTableColumn(string propertyName)
    {
        return string.IsNullOrEmpty(SourceTableAlias) ? TableInfor.CreateParameterName(propertyName) : $"{SourceTableAlias}.{Quote(propertyName)}";
    }
}
