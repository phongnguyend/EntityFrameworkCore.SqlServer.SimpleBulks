using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.Extensions
{
    public static class ObjectExtensions
    {
        public static List<SqlParameter> ToSqlParameters<T>(this T data, IEnumerable<string> propertyNames)
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

            var parameters = new List<SqlParameter>();

            foreach (PropertyDescriptor prop in updatablePros)
            {
                var type = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                var para = new SqlParameter($"@{prop.Name}", prop.GetValue(data) ?? DBNull.Value);

                if (type == typeof(DateTime))
                {
                    para.DbType = System.Data.DbType.DateTime2;
                }

                parameters.Add(para);
            }

            return parameters;
        }
    }
}
