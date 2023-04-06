using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Deveel.Data {
	static class EntityExtensions {
		private static bool TryGetMemberValue<TValue>(this object entity, string memberName, [MaybeNullWhen(false)] out TValue value) {
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

		public static bool TryGetId(this object entity, [MaybeNullWhen(false)] out string? value) {
			if (entity == null) {
				value = null;
				return false;
			}

			if (entity is IDataEntity dataEntity) {
				value = dataEntity.Id;
				return true;
			}

			if (TryGetMemberValue<string>(entity, "Id", out var id)) {
				value = id;
				return true; 
			}

			value = null;
			return false;
		}
	}
}
