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
    }
}
