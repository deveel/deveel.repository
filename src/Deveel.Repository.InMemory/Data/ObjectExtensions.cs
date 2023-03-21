using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;

namespace Deveel.Data {
	static class ObjectExtensions {
		public static bool TrySetMemberValue(this object entity, string memberName, object? value) {
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

		public static bool TryGetMemberValue<TValue>(this object entity, string memberName, [MaybeNullWhen(false)] out TValue value) {
			var binding = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic;
			var property = entity.GetType().GetProperty(memberName, binding);
			if (property == null || !property.CanRead) {
				value = default;
				return false;
			}

			var propValue = property.GetValue(entity, null);
			if (propValue == null) {
				value = default;
				return false;
			}

			if (propValue is TValue v) {
				value = v;
				return true;
			}

			value = (TValue) Convert.ChangeType(propValue, typeof(TValue), CultureInfo.InvariantCulture);
			return true;
		}
	}
}
