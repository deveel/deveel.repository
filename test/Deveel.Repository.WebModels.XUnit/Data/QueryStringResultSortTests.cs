using Microsoft.AspNetCore.Http;

namespace Deveel.Data {
	public static class QueryStringResultSortTests {
		[Theory]
		[InlineData("field1", "field1", true)]
		[InlineData("field1:asc", "field1", true)]
		[InlineData("field1:desc", "field1", false)]
		public static void ParseSingleResultSortInQuery(string queryString, string fieldName, bool asc) {
			var parsed = QueryStringResultSort.TryParse(queryString, out var sorts);

			Assert.True(parsed);
			Assert.NotNull(sorts);
			Assert.NotEmpty(sorts);
			Assert.Single(sorts);

			var sort = sorts[0];
			Assert.NotNull(sort);
			var fieldSort = Assert.IsType<FieldResultSort>(sort);

			Assert.Equal(fieldName, fieldSort.FieldName);
			Assert.Equal(asc, fieldSort.Ascending);
		}

		[Theory]
		[InlineData("field1:asc,field2:desc", "field1", true, "field2", false)]
		[InlineData("field1:desc,field2:asc", "field1", false, "field2", true)]
		[InlineData("field1,field2:asc", "field1", true, "field2", true)]
		[InlineData("field1:asc,field2", "field1", true, "field2", true)]
		public static void ParseMultipleResultSortsInQuery(string queryString, string fieldName1, bool asc1, string fieldName2, bool asc2) {
			var parsed = QueryStringResultSort.TryParse(queryString, out var sorts);

			Assert.True(parsed);
			Assert.NotNull(sorts);
			Assert.NotEmpty(sorts);
			Assert.Equal(2, sorts.Count);

			var sort1 = sorts[0];
			Assert.NotNull(sort1);
			var fieldSort1 = Assert.IsType<FieldResultSort>(sort1);
			Assert.Equal(fieldName1, fieldSort1.FieldName);
			Assert.Equal(asc1, fieldSort1.Ascending);

			var sort2 = sorts[1];
			Assert.NotNull(sort2);
			var fieldSort2 = Assert.IsType<FieldResultSort>(sort2);
			Assert.Equal(fieldName2, fieldSort2.FieldName);
			Assert.Equal(asc2, fieldSort2.Ascending);
		}

		[Theory]
		[InlineData("?page=1&sort=field1:asc", "field1", true)]
		[InlineData("?page=1&sort=field1:desc", "field1", false)]
		[InlineData("?page=1&sort=field1", "field1", true)]
		public static void ParseQueryStringWithOneItem(string queryString, string fieldName, bool asc) {
			var qs = new QueryString(queryString);

			var parsed = QueryStringResultSort.TryParseQueryString(qs, "sort", out var sorts);

			Assert.True(parsed);
			Assert.NotNull(sorts);
			Assert.NotEmpty(sorts);
			Assert.Single(sorts);

			var sort = sorts[0];
			Assert.NotNull(sort);

			var fieldSort = Assert.IsType<FieldResultSort>(sort);
			Assert.Equal(fieldName, fieldSort.FieldName);
			Assert.Equal(asc, fieldSort.Ascending);
		}

		[Theory]
		[InlineData("?page=1&sort=field1:asc,field2:desc", "field1", true, "field2", false)]
		[InlineData("?page=1&sort=field1:desc,field2:asc", "field1", false, "field2", true)]
		[InlineData("?page=1&sort=field1,field2:asc", "field1", true, "field2", true)]
		[InlineData("?page=1&sort=field1:asc,field2", "field1", true, "field2", true)]
		public static void ParseQueryStringWithMultipleItemsInOneParameter(string queryString, string fieldName1, bool asc1, string fieldName2, bool asc2) {
			var qs = new QueryString(queryString);
			var parsed = QueryStringResultSort.TryParseQueryString(qs, "sort", out var sorts);
			Assert.True(parsed);
			Assert.NotNull(sorts);
			Assert.NotEmpty(sorts);
			Assert.Equal(2, sorts.Count);
			var sort1 = sorts[0];
			Assert.NotNull(sort1);
			var fieldSort1 = Assert.IsType<FieldResultSort>(sort1);
			Assert.Equal(fieldName1, fieldSort1.FieldName);
			Assert.Equal(asc1, fieldSort1.Ascending);
			var sort2 = sorts[1];
			Assert.NotNull(sort2);
			var fieldSort2 = Assert.IsType<FieldResultSort>(sort2);
			Assert.Equal(fieldName2, fieldSort2.FieldName);
			Assert.Equal(asc2, fieldSort2.Ascending);
		}

		[Theory]
		[InlineData("?page=1&sort=field1:asc&sort=field2:desc", "field1", true, "field2", false)]
		[InlineData("?page=1&sort=field1:desc&sort=field2:asc", "field1", false, "field2", true)]
		[InlineData("?page=1&sort=field1&sort=field2:asc", "field1", true, "field2", true)]
		[InlineData("?page=1&sort=field1:asc&sort=field2", "field1", true, "field2", true)]
		public static void ParseQueryStringWithMultipleItemsInMultipleParameters(string queryString, string fieldName1, bool asc1, string fieldName2, bool asc2) {
			var qs = new QueryString(queryString);
			var parsed = QueryStringResultSort.TryParseQueryString(qs, "sort", out var sorts);
			Assert.True(parsed);
			Assert.NotNull(sorts);
			Assert.NotEmpty(sorts);
			Assert.Equal(2, sorts.Count);
			var sort1 = sorts[0];
			Assert.NotNull(sort1);
			var fieldSort1 = Assert.IsType<FieldResultSort>(sort1);
			Assert.Equal(fieldName1, fieldSort1.FieldName);
			Assert.Equal(asc1, fieldSort1.Ascending);
			var sort2 = sorts[1];
			Assert.NotNull(sort2);
			var fieldSort2 = Assert.IsType<FieldResultSort>(sort2);
			Assert.Equal(fieldName2, fieldSort2.FieldName);
			Assert.Equal(asc2, fieldSort2.Ascending);
		}
	}
}
