using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.Extensions
{
    public static class IListExtensions
    {
        public static DataTable ToDataTable<T>(this IEnumerable<T> data, List<string> propertyNames)
        {
            var properties = TypeDescriptor.GetProperties(typeof(T));

            var updatablePros = new List<PropertyDescriptor>();
            foreach (PropertyDescriptor prop in properties)
            {
                if (propertyNames.Contains(prop.Name))
                {
                    updatablePros.Add(prop);
                }
            }

            var table = new DataTable();
            foreach (PropertyDescriptor prop in updatablePros)
            {
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            }
            foreach (T item in data)
            {
                var row = table.NewRow();
                foreach (PropertyDescriptor prop in updatablePros)
                {
                    var value = prop.GetValue(item) ?? DBNull.Value;
                    row[prop.Name] = value;
                }
                table.Rows.Add(row);
            }
            return table;
        }
    }
}
