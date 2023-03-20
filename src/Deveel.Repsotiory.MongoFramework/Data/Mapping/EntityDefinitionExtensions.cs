using System.Reflection;

using MongoDB.Driver;

using MongoFramework.Infrastructure.Mapping;

namespace Deveel.Data.Mapping {
    public static class EntityDefinitionExtensions {
		public static bool IsVersioned(this IEntityDefinition definition)
			=> definition.GetAllProperties().Any(x => Attribute.IsDefined(x.PropertyInfo, typeof(VersionAttribute)));

		public static EntityVersionPropertry? GetVersionPropery(this IEntityDefinition definition) {
			foreach (var property in definition.GetAllProperties()) {
				var attr = property.PropertyInfo.GetCustomAttribute<VersionAttribute>();
				if (attr != null) {
					var format = attr.Format ?? VersionFormat.Default;

					if (format == VersionFormat.Default)
						format = VersionUtil.GetFormat(property.PropertyType);

					return new EntityVersionPropertry(property, format);
				}
			}

			return null;
		}

		public static FilterDefinition<TEntity>? CreateVersionFilterFromEntity<TEntity>(this IEntityDefinition definition, TEntity entity) 
			where TEntity : class {
			var property = definition.GetVersionPropery();
			if (property == null)
				return null;

			return Builders<TEntity>.Filter.Eq(property.ElementName, property.GetValue(entity));
		}
	}
}
