using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EntityFrameworkCore.SqlServer.SimpleBulks;

public abstract class TableInfor<T>
{
    public string Schema { get; private set; }

    public string Name { get; private set; }

    public string SchemaQualifiedTableName { get; private set; }

    public IReadOnlyList<string> PrimaryKeys { get; init; }

    public IReadOnlyList<string> PropertyNames { get; init; }

    public IReadOnlyList<string> InsertablePropertyNames { get; init; }

    public IReadOnlyDictionary<string, string> ColumnNameMappings { get; init; }

    public IReadOnlyDictionary<string, string> ColumnTypeMappings { get; init; }

    public IReadOnlyDictionary<string, ValueConverter> ValueConverters { get; init; }

    public OutputId OutputId { get; init; }

    public Discriminator Discriminator { get; init; }

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
        if (Discriminator != null && Discriminator.PropertyName == propertyName)
        {
            return Discriminator.PropertyType;
        }

        return PropertiesCache<T>.GetPropertyUnderlyingType(propertyName, ValueConverters);
    }

    public object GetProviderValue(string propertyName, T item)
    {
        if (Discriminator != null && Discriminator.PropertyName == propertyName)
        {
            return Discriminator.PropertyValue;
        }

        return PropertiesCache<T>.GetPropertyValue(propertyName, item, ValueConverters);
    }

    public string CreateParameterName(string propertyName)
    {
        if (propertyName.Contains('.'))
        {
            return $"@{propertyName.Replace(".", "_")}";
        }

        return $"@{propertyName}";
    }

    public IReadOnlyCollection<string> IncludeDiscriminator(IReadOnlyCollection<string> propertyNames)
    {
        if (Discriminator != null && !propertyNames.Contains(Discriminator.PropertyName))
        {
            var copiedPropertyNames = propertyNames.ToList();
            copiedPropertyNames.Add(Discriminator.PropertyName);
            return copiedPropertyNames;
        }

        return propertyNames;
    }

    public string CreateParameterNames(IReadOnlyCollection<string> propertyNames, bool includeDiscriminator)
    {
        var copiedPropertyNames = includeDiscriminator ? IncludeDiscriminator(propertyNames) : propertyNames;

        return string.Join(", ", copiedPropertyNames.Select(CreateParameterName));
    }

    public string CreateColumnNames(IReadOnlyCollection<string> propertyNames, bool includeDiscriminator)
    {
        var copiedPropertyNames = includeDiscriminator ? IncludeDiscriminator(propertyNames) : propertyNames;

        return string.Join(", ", copiedPropertyNames.Select(x => $"[{x}]"));
    }

    public string CreateColumnNames(IReadOnlyCollection<string> propertyNames, string tableName, bool includeDiscriminator)
    {
        var copiedPropertyNames = includeDiscriminator ? IncludeDiscriminator(propertyNames) : propertyNames;

        return string.Join(", ", copiedPropertyNames.Select(x => $"{tableName}.[{x}]"));
    }

    public string CreateDbColumnNames(IReadOnlyCollection<string> propertyNames, bool includeDiscriminator)
    {
        var copiedPropertyNames = includeDiscriminator ? IncludeDiscriminator(propertyNames) : propertyNames;

        return string.Join(", ", copiedPropertyNames.Select(x => $"[{GetDbColumnName(x)}]"));
    }

    public string CreateDbColumnNames(IReadOnlyCollection<string> propertyNames, string tableName, bool includeDiscriminator)
    {
        var copiedPropertyNames = includeDiscriminator ? IncludeDiscriminator(propertyNames) : propertyNames;

        return string.Join(", ", copiedPropertyNames.Select(x => $"{tableName}.[{GetDbColumnName(x)}]"));
    }

    public abstract List<ParameterInfo> CreateSqlParameters(SqlCommand command, T data, IReadOnlyCollection<string> propertyNames, bool includeDiscriminator, bool autoAdd);
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

    public override List<ParameterInfo> CreateSqlParameters(SqlCommand command, T data, IReadOnlyCollection<string> propertyNames, bool includeDiscriminator, bool autoAdd)
    {
        var parameters = new List<ParameterInfo>();

        var mappingSource = _dbContext.GetService<IRelationalTypeMappingSource>();

        foreach (var propName in propertyNames)
        {
            if (ColumnTypeMappings != null && ColumnTypeMappings.TryGetValue(propName, out var columnType))
            {
                var mapping = mappingSource.FindMapping(columnType);
                var para = (SqlParameter)mapping.CreateParameter(command, CreateParameterName(propName), GetProviderValue(propName, data) ?? DBNull.Value);

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

        if (includeDiscriminator && Discriminator != null && !propertyNames.Contains(Discriminator.PropertyName))
        {
            var mapping = mappingSource.FindMapping(Discriminator.ColumnType);
            var para = (SqlParameter)mapping.CreateParameter(command, CreateParameterName(Discriminator.PropertyName), Discriminator.PropertyValue ?? DBNull.Value);

            parameters.Add(new ParameterInfo
            {
                Name = para.ParameterName,
                Type = Discriminator.ColumnType,
                Parameter = para
            });

            if (autoAdd)
            {
                command.Parameters.Add(para);
            }
        }

        return parameters;

    }
}

public class SqlTableInfor<T> : TableInfor<T>
{
    public Func<T, string, SqlParameter> ParameterConverter { get; init; }

    public SqlTableInfor(string schema, string tableName) : base(schema, tableName)
    {
    }

    public SqlTableInfor(string tableName) : base(tableName)
    {
    }

    public override List<ParameterInfo> CreateSqlParameters(SqlCommand command, T data, IReadOnlyCollection<string> propertyNames, bool includeDiscriminator, bool autoAdd)
    {
        var parameters = new List<ParameterInfo>();

        foreach (var propName in propertyNames)
        {
            var para = ParameterConverter?.Invoke(data, propName);

            if (para == null)
            {
                para = new SqlParameter(CreateParameterName(propName), GetProviderValue(propName, data) ?? DBNull.Value);

                var paraInfo = new ParameterInfo
                {
                    Name = para.ParameterName,
                    Parameter = para
                };

                if (ColumnTypeMappings != null && ColumnTypeMappings.TryGetValue(propName, out var columnType))
                {
                    paraInfo.Type = columnType;
                }
                else
                {
                    var type = GetProviderClrType(propName);
                    paraInfo.Type = type.ToSqlDbType();
                }

                para.SqlDbType = paraInfo.Type.ToSqlDbType();

                parameters.Add(paraInfo);
            }
            else
            {
                parameters.Add(new ParameterInfo
                {
                    Name = para.ParameterName,
                    Type = para.SqlDbType.ToString(),
                    Parameter = para,
                    FromConverter = true
                });
            }

            if (autoAdd)
            {
                command.Parameters.Add(para);
            }
        }

        return parameters;

    }
}
