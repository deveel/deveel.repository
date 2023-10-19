using System.ComponentModel.DataAnnotations;

namespace Deveel.Data {
	public static class PageQueryTests {
		[Fact]
		public static void NewPageQuery() {
			var query = new PageQuery<Person>(1, 10);

			Assert.Equal(1, query.Page);
			Assert.Equal(10, query.Size);
			Assert.Equal(0, query.Offset);
			Assert.Null(query.Query);
		}

		[Fact]
		public static void NewPageQuery_WithEmptyFilter() {
			var query = new PageQuery<Person>(1, 10) {
				Query = Query.Empty
			};

			Assert.Equal(1, query.Page);
			Assert.Equal(10, query.Size);
			Assert.Equal(0, query.Offset);
			Assert.NotNull(query.Query);
			Assert.NotNull(query.Query.Value.Filter);
			Assert.Equal(QueryFilter.Empty, query.Query.Value.Filter);
			Assert.True(query.Query.Value.Filter.IsEmpty());
			Assert.Null(query.Query.Value.Sort);
		}

		[Fact]
		public static void NewPageQuery_WithExpressionFilter() {
			var query = new PageQuery<Person>(1, 10)
				.Where(x => x.FirstName == "John");

			Assert.Equal(1, query.Page);
			Assert.Equal(10, query.Size);
			Assert.Equal(0, query.Offset);
			Assert.NotNull(query.Query);
			Assert.True(query.Query.Value.HasFilter);
			Assert.NotNull(query.Query.Value.Filter);
			Assert.False(query.Query.Value.Filter.IsEmpty());

			var expr = Assert.IsType<ExpressionQueryFilter<Person>>(query.Query.Value.Filter);
			Assert.Equal("x => (x.FirstName == \"John\")", expr.Expression.ToString());

			Assert.Null(query.Query.Value.Sort);
		}

		[Fact]
		public static void NewPageQuery_WithCombinedFilter() {
			var query = new PageQuery<Person>(1, 10)
				.Where(x => x.FirstName == "John")
				.Where(x => x.LastName == "Doe");

			Assert.Equal(1, query.Page);
			Assert.Equal(10, query.Size);
			Assert.Equal(0, query.Offset);
			Assert.NotNull(query.Query);
			Assert.NotNull(query.Query.Value.Filter);
			Assert.False(query.Query.Value.Filter.IsEmpty());

			var filter = Assert.IsType<CombinedQueryFilter>(query.Query.Value.Filter);
			Assert.Equal(2, filter.Count());

			var expr1 = Assert.IsType<ExpressionQueryFilter<Person>>(filter.ElementAt(0));
			Assert.Equal("x => (x.FirstName == \"John\")", expr1.Expression.ToString());

			var expr2 = Assert.IsType<ExpressionQueryFilter<Person>>(filter.ElementAt(1));
			Assert.Equal("x => (x.LastName == \"Doe\")", expr2.Expression.ToString());
		}

		[Fact]
		public static void NewPageQuery_WithExpressionSort() {
			var query = new PageQuery<Person>(1, 10)
				.OrderBy(x => x.FirstName);

			Assert.Equal(1, query.Page);
			Assert.Equal(10, query.Size);
			Assert.Equal(0, query.Offset);
			Assert.NotNull(query.Query);
			Assert.NotNull(query.Query.Value.Filter);
			Assert.Equal(QueryFilter.Empty, query.Query.Value.Filter);
			Assert.False(query.Query.Value.HasFilter);
			Assert.NotNull(query.Query.Value.Sort);

			var expSort = Assert.IsType<ExpressionSort<Person>>(query.Query.Value.Sort);
			Assert.Equal("x => x.FirstName", expSort.Field.ToString());
		}

		[Fact]
		public static void NewPageQuery_WithCombinedSort() {
			var query = new PageQuery<Person>(1, 10)
				.OrderBy(x => x.FirstName)
				.OrderByDescending(x => x.LastName);

			Assert.Equal(1, query.Page);
			Assert.Equal(10, query.Size);
			Assert.Equal(0, query.Offset);
			Assert.NotNull(query.Query);
			Assert.False(query.Query.Value.HasFilter);
			Assert.NotNull(query.Query.Value.Filter);
			Assert.Equal(QueryFilter.Empty, query.Query.Value.Filter);
			Assert.NotNull(query.Query.Value.Sort);

			var combinedSort = Assert.IsType<CombinedSort>(query.Query.Value.Sort);

			Assert.Equal(2, combinedSort.Count());

			var sort1 = Assert.IsType<ExpressionSort<Person>>(combinedSort.ElementAt(0));
			Assert.True(sort1.IsAscending());
			Assert.Equal("x => x.FirstName", sort1.Field.ToString());

			var sort2 = Assert.IsType<ExpressionSort<Person>>(combinedSort.ElementAt(1));
			Assert.False(sort2.IsAscending());
			Assert.Equal("x => x.LastName", sort2.Field.ToString());
		}

		[Fact]
		public static void NewPageQuery_WithFieldSort() {
			var query = new PageQuery<Person>(1, 10)
				.OrderBy("FirstName");

			Assert.Equal(1, query.Page);
			Assert.Equal(10, query.Size);
			Assert.Equal(0, query.Offset);
			Assert.NotNull(query.Query);
			Assert.NotNull(query.Query.Value.Filter);
			Assert.Equal(QueryFilter.Empty, query.Query.Value.Filter);
			Assert.False(query.Query.Value.HasFilter);
			Assert.NotNull(query.Query.Value.Sort);

			var fieldSort = Assert.IsType<FieldSort>(query.Query.Value.Sort);

			Assert.Equal("FirstName", fieldSort.FieldName);
			Assert.True(fieldSort.IsAscending());
		}

		[Fact]
		public static void NewPageQuery_WithMulltiSort() {
			var query = new PageQuery<Person>(1, 10)
				.OrderBy(x => x.FirstName)
				.OrderByDescending("LastName");
		}
	}
}
