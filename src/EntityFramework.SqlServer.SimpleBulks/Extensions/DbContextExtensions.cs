using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;

namespace EntityFramework.SqlServer.SimpleBulks.Extensions
{
    public static class DbContextExtensions
    {
        public static string GetTableName(this DbContext dbContext, Type type)
        {
            var metadata = ((IObjectContextAdapter)dbContext).ObjectContext.MetadataWorkspace;

            // Get the part of the model that contains info about the actual CLR types
            var objectItemCollection = (ObjectItemCollection)metadata.GetItemCollection(DataSpace.OSpace);

            // Get the entity type from the model that maps to the CLR type
            var entityType = metadata
                    .GetItems<EntityType>(DataSpace.OSpace)
                          .Single(e => objectItemCollection.GetClrType(e) == type);

            IEnumerable<string> keyNames = entityType.KeyMembers.Select(k => k.Name);

            // Get the entity set that uses this entity type
            var entitySet = metadata
                .GetItems<EntityContainer>(DataSpace.CSpace)
                      .Single()
                      .EntitySets
                      .Single(s => s.ElementType.Name == entityType.Name);

            // Find the mapping between conceptual and storage model for this entity set
            var mapping = metadata.GetItems<EntityContainerMapping>(DataSpace.CSSpace)
                          .Single()
                          .EntitySetMappings
                          .Single(s => s.EntitySet == entitySet);

            // Find the storage entity set (table) that the entity is mapped
            var tableEntitySet = mapping
                .EntityTypeMappings.Single()
                .Fragments.Single()
                .StoreEntitySet;

            // Return the table name from the storage entity set
            var tableName = tableEntitySet.MetadataProperties["Table"].Value ?? tableEntitySet.Name;
            return tableName.ToString();
        }

        public static SqlConnection GetSqlConnection(this DbContext dbContext)
        {
            return dbContext.Database.Connection.AsSqlConnection();
        }

        public static SqlTransaction GetCurrentSqlTransaction(this DbContext dbContext)
        {
            var transaction = dbContext.Database.CurrentTransaction;
            return transaction == null ? null : transaction.UnderlyingTransaction as SqlTransaction;
        }

        // https://romiller.com/2014/04/08/ef6-1-mapping-between-types-tables/
        // https://romiller.com/2014/10/07/ef6-1-getting-key-properties-for-an-entity/
        // https://romiller.com/2015/08/05/ef6-1-get-mapping-between-properties-and-columns/
        public static IList<(string PropertyName, Type PropertyType, string ColumnName, string ColumnType, StoreGeneratedPattern ValueGenerated, bool IsPrimaryKey)> GetProperties(this DbContext dbContext, Type type)
        {
            var typeProperties = type.GetProperties().Select(x => new { x.Name, x.PropertyType });

            var metadata = ((IObjectContextAdapter)dbContext).ObjectContext.MetadataWorkspace;

            // Get the part of the model that contains info about the actual CLR types
            var objectItemCollection = (ObjectItemCollection)metadata.GetItemCollection(DataSpace.OSpace);

            // Get the entity type from the model that maps to the CLR type
            var entityType = metadata
                    .GetItems<EntityType>(DataSpace.OSpace)
                          .Single(e => objectItemCollection.GetClrType(e) == type);

            IEnumerable<string> keyNames = entityType.KeyMembers.Select(k => k.Name);

            // Get the entity set that uses this entity type
            var entitySet = metadata
                .GetItems<EntityContainer>(DataSpace.CSpace)
                      .Single()
                      .EntitySets
                      .Single(s => s.ElementType.Name == entityType.Name);

            // Find the mapping between conceptual and storage model for this entity set
            var mapping = metadata.GetItems<EntityContainerMapping>(DataSpace.CSSpace)
                          .Single()
                          .EntitySetMappings
                          .Single(s => s.EntitySet == entitySet);

            // Find the storage entity set (table) that the entity is mapped
            var tableEntitySet = mapping
                .EntityTypeMappings.Single()
                .Fragments.Single()
                .StoreEntitySet;

            // Return the table name from the storage entity set
            var tableName = tableEntitySet.MetadataProperties["Table"].Value ?? tableEntitySet.Name;

            // Find the storage property (column) that the property is mapped
            var entityProperties = mapping
                            .EntityTypeMappings.Single()
                            .Fragments.Single()
                            .PropertyMappings
                            .OfType<ScalarPropertyMapping>()
                            .Select(m => new
                            {
                                Name = m.Property.Name,
                                ColumnName = m.Column.Name,
                                ColumnType = m.Column.TypeName,
                                IsPrimaryKey = keyNames.Contains(m.Property.Name),
                                ValueGenerated = m.Column.StoreGeneratedPattern
                            });

            var data = typeProperties.Join(entityProperties,
                prop => prop.Name,
                entityProp => entityProp.Name,
                (prop, entityProp) => (
                    PropertyName: prop.Name,
                    prop.PropertyType,
                    entityProp.ColumnName,
                    entityProp.ColumnType,
                    entityProp.ValueGenerated,
                    entityProp.IsPrimaryKey
                ));

            return data.ToList();
        }

        // https://romiller.com/2014/04/08/ef6-1-mapping-between-types-tables/
        // https://romiller.com/2014/10/07/ef6-1-getting-key-properties-for-an-entity/
        // https://romiller.com/2015/08/05/ef6-1-get-mapping-between-properties-and-columns/
        public static Dictionary<string, string> GetMappedColumns(this DbContext dbContext, Type type)
        {
            var typeProperties = type.GetProperties().Select(x => new { x.Name, x.PropertyType });

            var metadata = ((IObjectContextAdapter)dbContext).ObjectContext.MetadataWorkspace;

            // Get the part of the model that contains info about the actual CLR types
            var objectItemCollection = (ObjectItemCollection)metadata.GetItemCollection(DataSpace.OSpace);

            // Get the entity type from the model that maps to the CLR type
            var entityType = metadata
                    .GetItems<EntityType>(DataSpace.OSpace)
                          .Single(e => objectItemCollection.GetClrType(e) == type);

            IEnumerable<string> keyNames = entityType.KeyMembers.Select(k => k.Name);

            // Get the entity set that uses this entity type
            var entitySet = metadata
                .GetItems<EntityContainer>(DataSpace.CSpace)
                      .Single()
                      .EntitySets
                      .Single(s => s.ElementType.Name == entityType.Name);

            // Find the mapping between conceptual and storage model for this entity set
            var mapping = metadata.GetItems<EntityContainerMapping>(DataSpace.CSSpace)
                          .Single()
                          .EntitySetMappings
                          .Single(s => s.EntitySet == entitySet);

            // Find the storage entity set (table) that the entity is mapped
            var tableEntitySet = mapping
                .EntityTypeMappings.Single()
                .Fragments.Single()
                .StoreEntitySet;

            // Return the table name from the storage entity set
            var tableName = tableEntitySet.MetadataProperties["Table"].Value ?? tableEntitySet.Name;

            // Find the storage property (column) that the property is mapped
            var entityProperties = mapping
                .EntityTypeMappings.Single()
                .Fragments.Single()
                .PropertyMappings
                .OfType<ScalarPropertyMapping>()
                .Select(m => new { Name = m.Property.Name, ColumnName = m.Column.Name, ColumnType = m.Column.TypeName, IsPrimaryKey = keyNames.Contains(m.Property.Name), ValueGenerated = m.Column.StoreGeneratedPattern });

            var data = typeProperties.Join(entityProperties, prop => prop.Name, entityProp => entityProp.Name, (prop, entityProp) => new { prop.Name, prop.PropertyType, entityProp.ColumnName, entityProp.ColumnType });

            return data.ToDictionary(x => x.Name, x => x.ColumnName);
        }
    }
}
