using System.Diagnostics.CodeAnalysis;
using System.Text;

using Microsoft.AspNetCore.Http;

namespace Deveel.Data {
	/// <summary>
	/// Provides utility methods to parse and format a strings
	/// of sorting rules for query strings of HTTP requests.
	/// </summary>
	public static class QueryStringResultSort {
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
			string? dir = null;
			bool ascending = true;

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

			if (!string.IsNullOrWhiteSpace(dir)) {
				if (String.Equals(dir, "DESC", StringComparison.OrdinalIgnoreCase)) {
					ascending = false;
				} else if (String.Equals(dir, "ASC", StringComparison.OrdinalIgnoreCase)) {
					ascending = true;
				} else {
					sort = null;
					error = new FormatException($"The sort direction '{dir}' is invalid");
					return false;
				}
			}

			sort = new FieldResultSort(field, ascending);
			error = null;
			return true;
		}

		/// <summary>
		/// Attempts to parse the given string as a list of sorting rules
		/// </summary>
		/// <param name="s">
		/// The string to parse.
		/// </param>
		/// <param name="results">
		/// The list of sorting rules parsed from the string.
		/// </param>
		/// <returns>
		/// Returns <c>true</c> if the string was successfully parsed,
		/// and the <paramref name="results"/> contains the list of sorting
		/// rules, otherwise <c>false</c>.
		/// </returns>
		public static bool TryParse(string s, [MaybeNullWhen(false)] out IList<IResultSort> results) {
			if (!TryParse(s, out results, out var error))
				return false;

			return true;
		}

		/// <summary>
		/// Parses the given string as a list of sorting rules
		/// </summary>
		/// <param name="s">The string to be parsed.</param>
		/// <returns>
		/// Returns a list of sorting rules parsed from the given string.
		/// </returns>
		/// <exception cref="FormatException">
		/// Thrown if the given string is not a valid list of sorting rules
		/// </exception>
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

		public static bool TryParseQueryString(string queryString, string paramName, [MaybeNullWhen(false)] out IList<IResultSort> sorts)
			=> TryParseQueryString(new QueryString(queryString), paramName, out sorts);

		public static bool TryParseQueryString(QueryString queryString, string paramName, [MaybeNullWhen(false)] out IList<IResultSort> sorts) {
			if (!queryString.HasValue) {
				sorts = null;
				return false;
			}

			var qs = queryString.ToUriComponent();
			var items = qs.Split('&', StringSplitOptions.RemoveEmptyEntries)
				.Select(s => s.Split('='))
				.Where(s => !String.IsNullOrWhiteSpace(s[0]) && s[0] == paramName)
				.Select(s => s[1])
				.ToList();
			if (items.Count == 0) {
				sorts = null;
				return false;
			}

			sorts = new List<IResultSort>();
			foreach (var item in items) {
				if (!TryParse(item, out var itemSorts, out var error))
					return false;

				foreach (var sort in itemSorts) {
					sorts.Add(sort);
				}
			}

			return true;
		}
	}
}
