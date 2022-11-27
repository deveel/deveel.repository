using System;
using System.Reflection;

using MongoFramework.Infrastructure.Mapping;

namespace Deveel.Data.Mapping {
	public sealed class EntityVersionPropertry : IEntityProperty {
		private readonly IEntityProperty property;

		internal EntityVersionPropertry(IEntityProperty property, VersionFormat format) {
			this.property = property;
			Format = format;
		}

		public Type EntityType => property.EntityType;

		public bool IsKey => property.IsKey;

		public string ElementName => property.ElementName;

		public string FullPath => property.FullPath;

		public Type PropertyType => property.PropertyType;

		public PropertyInfo PropertyInfo => property.PropertyInfo;

		public VersionFormat Format { get; }

		public object GetValue(object entity) => property.GetValue(entity);

		public void SetValue(object entity, object value) => property.SetValue(entity, value);
	}
}
