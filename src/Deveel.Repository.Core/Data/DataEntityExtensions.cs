using System.Globalization;
using System.Reflection;

namespace Deveel.Data {
	/// <summary>
	/// Extension methods to the <see cref="IDataEntity"/>
	/// </summary>
    public static class DataEntityExtensions {
		/// <summary>
		/// Tries to set the given value to a member of the entity instance
		/// </summary>
		/// <typeparam name="TEntity">The type of entity</typeparam>
		/// <param name="entity">The instance of the entity to set a member value</param>
		/// <param name="memberName">The name of the member of the entity to be set</param>
		/// <param name="value">The value to be set</param>
		/// <returns>
		/// Returns <c>true</c> that indicates if the member value was set,
		/// otherwise returns <c>false</c>.
		/// </returns>
        public static bool TrySetMemberValue<TEntity>(this TEntity entity, string memberName, object value) 
			where TEntity : class, IDataEntity {
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
