using System;

namespace Deveel.Data {
	static class VersionUtil {
		public static VersionFormat GetFormat(Type propertyType) {
			if (propertyType == typeof(Guid) ||
				propertyType == typeof(string))
				return VersionFormat.Guid;
			if (propertyType == typeof(int) ||
				propertyType == typeof(long))
				return VersionFormat.Integer;

			throw new NotSupportedException($"The type '{propertyType}' is not valid to handle versions");
		}

		private static object FormatInteger(object value) {
			if (value is int i)
				return i;
			if (value is long l)
				return l;

			if (value is string s1 && Int64.TryParse(s1, out var l2))
				return l2;
			if (value is string s2 && Int32.TryParse(s2, out var i2))
				return i2;

			throw new NotSupportedException($"The value {value} is not a valid version integer");
		}

		private static object FormatGuid(object value) {
			if (value is Guid guid)
				return guid;
			if (value is string s)
				return s;

			throw new NotSupportedException($"The value {value} is not a valid version GUID");
		}

		public static object Format(object value, VersionFormat format) {
			if (format == VersionFormat.Integer) {
				return FormatInteger(value);
			} else if (format == VersionFormat.Guid) {
				return FormatGuid(value);
			}

			throw new NotSupportedException();
		}

		public static object GetNewVersion(VersionFormat format, Type propertyType, object currentValue) {
			if (format == VersionFormat.Guid)
				return GetNewGuid(propertyType);
			if (format == VersionFormat.Integer)
				return GetNewInteger(propertyType, currentValue);

			throw new NotSupportedException();
		}

		private static object GetNewGuid(Type propertyType) {
			if (propertyType == typeof(Guid))
				return Guid.NewGuid();
			if (propertyType == typeof(string))
				return Guid.NewGuid().ToString("N");

			throw new NotSupportedException();
		}

		private static object GetNewInteger(Type propertyType, object currentValue) {
			if (propertyType == typeof(int)) {
				if (currentValue == null)
					return 0;
				if (currentValue is int i)
					return i + 1;
				if (currentValue is long l)
					return (int) l + 1;
			}

			if (propertyType == typeof(long)) {
				if (currentValue == null)
					return 0L;
				if (currentValue is int i)
					return (long)(i + 1);
				if (currentValue is long l)
					return l + 1;
			}

			throw new NotSupportedException("Unable to increment a non integer");
		}
	}
}
