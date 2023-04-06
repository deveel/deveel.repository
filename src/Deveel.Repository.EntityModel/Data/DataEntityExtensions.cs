using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;

namespace Deveel.Data {
    public static class DataEntityExtensions {
        public static bool TrySetMemberValue<TEntity>(this TEntity entity, string memberName, object value) where TEntity : class, IDataEntity {
            var binding = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic;
            var property = entity.GetType().GetProperty(memberName, binding);
            if (property == null || !property.CanWrite)
                return false;

            var propertyType = property.PropertyType;
            if (!propertyType.IsInstanceOfType(value)) {
                // TODO: find a better way?
                value = Convert.ChangeType(value, propertyType, CultureInfo.InvariantCulture);
            }

            property.SetValue(entity, value, null);
            return true;
        }

		private static bool TryGetMemberValue(this IDataEntity entity, string memberName, [MaybeNullWhen(false)] out string? tenantId) {
			var binding = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic;
			var property = entity.GetType().GetProperty(memberName, binding);
			if (property == null || !property.CanRead) {
				tenantId = null;
				return false;
			}

			var value = property.GetValue(entity, null);
			if (value == null) {
				tenantId = null;
				return false;
			}

			if (value is string s) {
				tenantId = s;
				return true;
			}

			tenantId = Convert.ToString(value, CultureInfo.InvariantCulture);
			return true;
		}

		public static string? TenantId<TEntity>(this TEntity entity) where TEntity : class, IDataEntity {
			if (entity == null) 
				return null;
			if (entity is ITenantDataEntity tenantEntity)
				return tenantEntity.TenantId;

			if (entity.TryGetMemberValue(nameof(ITenantDataEntity.TenantId), out var tenantId))
				return tenantId;

			return null;
		}
    }
}
