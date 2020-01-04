using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.SqlConverters
{
    public static class MappingConfigurationFactory
    {
        private static List<IMappingConfiguration<dynamic>> _mappings = new List<IMappingConfiguration<dynamic>>();

        static MappingConfigurationFactory()
        {
            var types = Assembly.GetEntryAssembly()
                .GetReferencedAssemblies()
                .Select(Assembly.Load)
                .Append(Assembly.GetEntryAssembly())
                .SelectMany(assembly => assembly.DefinedTypes);

            foreach (Type type in types)
            {
                if (!type.IsInterface
                    && typeof(IMappingConfiguration<>).IsAssignableFrom(type))
                {
                    _mappings.Add((IMappingConfiguration<dynamic>)Activator.CreateInstance(type));
                }
            }
        }

        public static IMappingConfiguration<dynamic> GetMappingConfiguration(Type type)
        {
            throw new NotImplementedException();
        }
    }
}
