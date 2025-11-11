using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace EntityFrameworkCore.SqlServer.SimpleBulks;

public abstract class TableInfor<T>
{
    public string Schema { get; private set; }

    public string Name { get; private set; }

    public string SchemaQualifiedTableName { get; private set; }

    public IReadOnlyList<string> PrimaryKeys { get; init; }

    public IReadOnlyList<string> PropertyNames { get; init; }

    public IReadOnlyList<string> InsertablePropertyNames { get; init; }

    public IReadOnlyDictionary<string, Type> PropertyTypes { get; init; }

    public IReadOnlyDictionary<string, string> ColumnNameMappings { get; init; }

    public IReadOnlyDictionary<string, string> ColumnTypeMappings { get; init; }

    public IReadOnlyDictionary<string, ValueConverter> ValueConverters { get; init; }

    public OutputId OutputId { get; init; }

    public TableInfor(string schema, string tableName)
    {
        Schema = schema;
        Name = tableName;
        SchemaQualifiedTableName = string.IsNullOrEmpty(schema) ? $"[{tableName}]" : $"[{schema}].[{tableName}]";
    }

    public TableInfor(string tableName) : this(null, tableName)
    {
    }

    public string GetDbColumnName(string propertyName)
    {
        if (ColumnNameMappings == null)
        {
            return propertyName;
        }

        return ColumnNameMappings.TryGetValue(propertyName, out string value) ? value : propertyName;
    }

    public Type GetProviderClrType(string propertyName)
    {
        if (ValueConverters != null && ValueConverters.TryGetValue(propertyName, out var converter))
        {
            return Nullable.GetUnderlyingType(converter.ProviderClrType) ?? converter.ProviderClrType;
        }

        if (PropertyTypes != null && PropertyTypes.TryGetValue(propertyName, out var type))
        {
            return Nullable.GetUnderlyingType(type) ?? type;
        }

        var property = PropertiesCache<T>.GetProperty(propertyName);
        if (property != null)
        {
            return Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
        }

        throw new ArgumentException($"Property '{propertyName}' not found.");
    }

    public abstract List<ParameterInfo> CreateSqlParameters(SqlCommand command, T data, IEnumerable<string> propertyNames, bool autoAdd);
}

public class DbContextTableInfor<T> : TableInfor<T>
{
    private readonly DbContext _dbContext;

    public DbContextTableInfor(string schema, string tableName, DbContext dbContext) : base(schema, tableName)
    {
        _dbContext = dbContext;
    }

    public DbContextTableInfor(string tableName, DbContext dbContext) : base(tableName)
    {
        _dbContext = dbContext;
    }

    public override List<ParameterInfo> CreateSqlParameters(SqlCommand command, T data, IEnumerable<string> propertyNames, bool autoAdd)
    {
        var parameters = new List<ParameterInfo>();

        var mappingSource = _dbContext.GetService<IRelationalTypeMappingSource>();

        foreach (var propName in propertyNames)
        {
            var prop = PropertiesCache<T>.GetProperty(propName);

            if (ColumnTypeMappings != null && ColumnTypeMappings.TryGetValue(prop.Name, out var columnType))
            {
                var mapping = mappingSource.FindMapping(columnType);
                var para = (SqlParameter)mapping.CreateParameter(command, $"@{prop.Name}", GetProviderValue(prop, data) ?? DBNull.Value);

                parameters.Add(new ParameterInfo
                {
                    Name = para.ParameterName,
                    Type = columnType,
                    Parameter = para
                });

                if (autoAdd)
                {
                    command.Parameters.Add(para);
                }
            }
        }

        return parameters;

    }

    private object GetProviderValue(PropertyInfo property, T item)
    {
        if (ValueConverters != null && ValueConverters.TryGetValue(property.Name, out var converter))
        {
            return converter.ConvertToProvider(property.GetValue(item));
        }

        return property.GetValue(item);
    }
}

public class SqlTableInfor<T> : TableInfor<T>
{
    public SqlTableInfor(string schema, string tableName) : base(schema, tableName)
    {
    }

    public SqlTableInfor(string tableName) : base(tableName)
    {
    }

    public override List<ParameterInfo> CreateSqlParameters(SqlCommand command, T data, IEnumerable<string> propertyNames, bool autoAdd)
    {
        var parameters = new List<ParameterInfo>();

        foreach (var propName in propertyNames)
        {
            var prop = PropertiesCache<T>.GetProperty(propName);

            var type = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

            var para = new SqlParameter($"@{prop.Name}", prop.GetValue(data) ?? DBNull.Value);

            var paraInfo = new ParameterInfo
            {
                Name = para.ParameterName,
                Parameter = para
            };

            if (ColumnTypeMappings != null && ColumnTypeMappings.TryGetValue(prop.Name, out var columnType))
            {
                paraInfo.Type = columnType;
            }
            else
            {
                paraInfo.Type = type.ToSqlDbType();
            }

            para.SqlDbType = paraInfo.Type.ToSqlDbType();

            parameters.Add(paraInfo);

            if (autoAdd)
            {
                command.Parameters.Add(para);
            }
        }

        return parameters;

    }
}
