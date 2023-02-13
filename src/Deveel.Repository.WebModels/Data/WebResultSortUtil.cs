using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Deveel.Data {
	public static class WebResultSortUtil {
		private static bool TryParse(string s, [MaybeNullWhen(false)] out IList<IResultSort> result, [MaybeNullWhen(true)] out Exception error) {
			if (String.IsNullOrWhiteSpace(s)) {
				error = new ArgumentNullException(nameof(s), "The string must be not null and not empty");
				result = null;
				return false;
			}

			var parts = s.Split(',', StringSplitOptions.RemoveEmptyEntries);

			result = new List<IResultSort>(parts.Length);

			foreach (var part in parts) {
				if (!TryParseSort(part, out var sort, out error)) {
					result = null;
					return false;
				}

				if (sort != null)
					result.Add(sort);
			}

			error = null;
			return true;
		}

		private static bool TryParseSort(string part, [MaybeNullWhen(false)] out IResultSort sort, [MaybeNullWhen(true)] out Exception error) {
			if (String.IsNullOrWhiteSpace(part)) {
				error = new FormatException("The sort part is empty or null");
				sort = null;
				return false;
			}

			var field = part;
			string dir = "desc";

			var idx = part.IndexOf(':');
			if (idx != -1) {
				dir = field.Substring(idx + 1);
				field = field.Substring(0, idx);
			}

			if (String.IsNullOrWhiteSpace(field)) {
				error = new FormatException("The field part of the sort is null or empty");
				sort = null;
				return false;
			}

			bool ascending = false;

			if (String.Equals(dir, "DESC", StringComparison.OrdinalIgnoreCase)) {
				ascending = false;
			} else if (String.Equals(dir, "ASC", StringComparison.OrdinalIgnoreCase)) {
				ascending = true;
			} else {
				sort = null;
				error = new FormatException($"The sort direction '{dir}' is invalid");
				return false;
			}

			sort = new ResultSortImpl(new StringFieldRef(field), ascending);
			error = null;
			return true;
		}

		public static bool TryParse(string s, out IList<IResultSort>? results) {
			if (!TryParse(s, out results, out var error))
				return false;

			return true;
		}

		public static IList<IResultSort> Parse(string s) {
			if (!TryParse(s, out var result, out var error))
				throw error;

			return result;
		}

		public static IResultSort ParseSort(string s) {
			if (!TryParseSort(s, out var sort, out var error))
				throw error;

			return sort;
		}

		public static string ToQueryString(string itemName, IEnumerable<IResultSort> sorts)
			=> String.Join("&", sorts.Select(s => ToQueryStringItem(itemName, s)));

		public static string ToQueryStringItem(string itemName, IResultSort sort) {
			var sb = new StringBuilder();
			sb.Append(itemName);
			sb.Append('=');
			sb.Append(sort.Field.ToString());

			if (sort.Ascending)
				sb.Append(':').Append("asc");

			return sb.ToString();
		}

		private class ResultSortImpl : IResultSort {
			public ResultSortImpl(IFieldRef fieldRef, bool ascending) {
				Field = fieldRef;
				Ascending = ascending;
			}

			public IFieldRef Field { get; }

			public bool Ascending { get; }
		}
	}
}
