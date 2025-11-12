using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks;

public class SqlTableInforBuilder<T>
{
    private string _schema;

    private string _name;

    private IReadOnlyList<string> _primaryKeys;

    private IReadOnlyList<string> _propertyNames;

    private IReadOnlyList<string> _insertablePropertyNames;

    private IReadOnlyDictionary<string, string> _columnNameMappings;

    private IReadOnlyDictionary<string, string> _columnTypeMappings;

    private IReadOnlyDictionary<string, ValueConverter> _valueConverters;

    private OutputId _outputId;

    private Func<T, string, SqlParameter> _parameterConverter;

    public SqlTableInforBuilder<T> Schema(string schema)
    {
        _schema = schema;
        return this;
    }

    public SqlTableInforBuilder<T> TableName(string name)
    {
        _name = name;
        return this;
    }

    public SqlTableInforBuilder<T> PrimaryKeys(IReadOnlyList<string> primaryKeys)
    {
        _primaryKeys = primaryKeys;
        return this;
    }

    public SqlTableInforBuilder<T> PrimaryKeys(Expression<Func<T, object>> primaryKeysSelector)
    {
        var primaryKey = primaryKeysSelector.Body.GetMemberName();
        var primaryKeys = string.IsNullOrEmpty(primaryKey) ? primaryKeysSelector.Body.GetMemberNames() : [primaryKey];
        return PrimaryKeys(primaryKeys);
    }

    public SqlTableInforBuilder<T> PropertyNames(IReadOnlyList<string> propertyNames)
    {
        _propertyNames = propertyNames;
        return this;
    }

    public SqlTableInforBuilder<T> PropertyNames(Expression<Func<T, object>> propertyNamesSelector)
    {
        var propertyName = propertyNamesSelector.Body.GetMemberName();
        var propertyNames = string.IsNullOrEmpty(propertyName) ? propertyNamesSelector.Body.GetMemberNames() : [propertyName];
        return PropertyNames(propertyNames);
    }

    public SqlTableInforBuilder<T> InsertablePropertyNames(IReadOnlyList<string> insertablePropertyNames)
    {
        _insertablePropertyNames = insertablePropertyNames;
        return this;
    }

    public SqlTableInforBuilder<T> ColumnNameMappings(IReadOnlyDictionary<string, string> columnNameMappings)
    {
        _columnNameMappings = columnNameMappings;
        return this;
    }

    public SqlTableInforBuilder<T> ColumnTypeMappings(IReadOnlyDictionary<string, string> columnTypeMappings)
    {
        _columnTypeMappings = columnTypeMappings;
        return this;
    }

    public SqlTableInforBuilder<T> ValueConverters(IReadOnlyDictionary<string, ValueConverter> valueConverters)
    {
        _valueConverters = valueConverters;
        return this;
    }

    public SqlTableInforBuilder<T> OutputId(string name, OutputIdMode outputIdMode)
    {
        _outputId = new OutputId
        {
            Name = name,
            Mode = outputIdMode
        };
        return this;
    }

    public SqlTableInforBuilder<T> OutputId(Expression<Func<T, object>> nameSelector, OutputIdMode outputIdMode)
    {
        var propertyName = nameSelector.Body.GetMemberName();
        return OutputId(propertyName, outputIdMode);
    }

    public SqlTableInforBuilder<T> ParameterConverter(Func<T, string, SqlParameter> converter)
    {
        _parameterConverter = converter;
        return this;
    }

    public SqlTableInfor<T> Build()
    {
        var tableInfor = new SqlTableInfor<T>(_schema, _name)
        {
            PrimaryKeys = _primaryKeys,
            PropertyNames = _propertyNames,
            InsertablePropertyNames = _insertablePropertyNames,
            ColumnNameMappings = _columnNameMappings,
            ColumnTypeMappings = _columnTypeMappings,
            ValueConverters = _valueConverters,
            OutputId = _outputId,
            ParameterConverter = _parameterConverter,
        };
        return tableInfor;
    }
}
