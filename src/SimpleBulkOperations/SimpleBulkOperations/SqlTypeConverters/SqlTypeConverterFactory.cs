using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SimpleBulkOperations.SqlTypeConverters
{
    public static class SqlTypeConverterFactory
    {
        private static List<ISqlTypeConvertible> supportedConverters = new List<ISqlTypeConvertible>();
        static SqlTypeConverterFactory()
        {
            var types = Assembly.GetEntryAssembly()
                .GetReferencedAssemblies()
                .Select(Assembly.Load)
                .Append(Assembly.GetEntryAssembly())
                .SelectMany(assembly => assembly.DefinedTypes);

            foreach (Type type in types)
            {
                if (!type.IsInterface
                    && typeof(ISqlTypeConvertible).IsAssignableFrom(type)
                    && !type.Equals(typeof(DefaultConverter)))
                {
                    supportedConverters.Add((ISqlTypeConvertible)Activator.CreateInstance(type));
                }
            }
        }

        public static ISqlTypeConvertible GetConverter(Type type)
        {
            foreach (var converter in supportedConverters)
            {
                if (converter.CanConvert(type))
                    return converter;
            }

            return new DefaultConverter();
        }
    }
}
