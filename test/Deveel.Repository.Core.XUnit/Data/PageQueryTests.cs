namespace Deveel.Data {
	public static class PageQueryTests {
		[Fact]
		public static void NewPageQuery() {
			var query = new PageQuery<Person>(1, 10);

			Assert.Equal(1, query.Page);
			Assert.Equal(10, query.Size);
			Assert.Equal(0, query.Offset);
			Assert.Null(query.Filter);
			Assert.Null(query.ResultSorts);
		}

		[Fact]
		public static void NewPageQuery_WithEmptyFilter() {
			var query = new PageQuery<Person>(1, 10) {
				Filter = QueryFilter.Empty
			};

			Assert.Equal(1, query.Page);
			Assert.Equal(10, query.Size);
			Assert.Equal(0, query.Offset);
			Assert.NotNull(query.Filter);
			Assert.Equal(QueryFilter.Empty, query.Filter);
			Assert.True(query.Filter.IsEmpty());
			Assert.Null(query.ResultSorts);
		}

		[Fact]
		public static void NewPageQuery_WithExpressionFilter() {
			var query = new PageQuery<Person>(1, 10)
				.Where(x => x.FirstName == "John");

			Assert.Equal(1, query.Page);
			Assert.Equal(10, query.Size);
			Assert.Equal(0, query.Offset);
			Assert.NotNull(query.Filter);
			Assert.False(query.Filter.IsEmpty());

			var expr = Assert.IsType<ExpressionQueryFilter<Person>>(query.Filter);
			Assert.Equal("x => (x.FirstName == \"John\")", expr.Expression.ToString());

			Assert.Null(query.ResultSorts);
		}

		[Fact]
		public static void NewPageQuery_WithCombinedFilter() {
			var query = new PageQuery<Person>(1, 10)
				.Where(x => x.FirstName == "John")
				.Where(x => x.LastName == "Doe");

			Assert.Equal(1, query.Page);
			Assert.Equal(10, query.Size);
			Assert.Equal(0, query.Offset);
			Assert.NotNull(query.Filter);
			Assert.False(query.Filter.IsEmpty());

			var filter = Assert.IsType<CombinedQueryFilter>(query.Filter);
			Assert.Equal(2, filter.Filters.Count);

			var expr1 = Assert.IsType<ExpressionQueryFilter<Person>>(filter.Filters[0]);
			Assert.Equal("x => (x.FirstName == \"John\")", expr1.Expression.ToString());

			var expr2 = Assert.IsType<ExpressionQueryFilter<Person>>(filter.Filters[1]);
			Assert.Equal("x => (x.LastName == \"Doe\")", expr2.Expression.ToString());
		}

		[Fact]
		public static void NewPageQuery_WithExpressionSort() {
			var query = new PageQuery<Person>(1, 10)
				.OrderBy(x => x.FirstName);

			Assert.Equal(1, query.Page);
			Assert.Equal(10, query.Size);
			Assert.Equal(0, query.Offset);
			Assert.Null(query.Filter);
			Assert.NotNull(query.ResultSorts);
			Assert.Single(query.ResultSorts);

			var sort = query.ResultSorts[0];
			Assert.IsType<ExpressionFieldRef<Person>>(sort.Field);

			var expSort = Assert.IsType<ExpressionResultSort<Person>>(query.ResultSorts[0]);
			Assert.Equal("x => x.FirstName", expSort.FieldSelector.ToString());
		}

		[Fact]
		public static void NewPageQuery_WithCombinedSort() {
			var query = new PageQuery<Person>(1, 10)
				.OrderBy(x => x.FirstName)
				.OrderByDescending(x => x.LastName);

			Assert.Equal(1, query.Page);
			Assert.Equal(10, query.Size);
			Assert.Equal(0, query.Offset);
			Assert.Null(query.Filter);
			Assert.NotNull(query.ResultSorts);
			Assert.Equal(2, query.ResultSorts.Count);

			var sort1 = query.ResultSorts[0];
			Assert.IsType<ExpressionFieldRef<Person>>(sort1.Field);
			Assert.True(sort1.Ascending);

			var expSort1 = Assert.IsType<ExpressionResultSort<Person>>(query.ResultSorts[0]);
			Assert.Equal("x => x.FirstName", expSort1.FieldSelector.ToString());

			var sort2 = query.ResultSorts[1];
			Assert.IsType<ExpressionFieldRef<Person>>(sort2.Field);
			Assert.False(sort2.Ascending);

			var expSort2 = Assert.IsType<ExpressionResultSort<Person>>(query.ResultSorts[1]);
			Assert.Equal("x => x.LastName", expSort2.FieldSelector.ToString());
		}

		[Fact]
		public static void NewPageQuery_WithFieldSort() {
			var query = new PageQuery<Person>(1, 10)
				.OrderBy("FirstName");

			Assert.Equal(1, query.Page);
			Assert.Equal(10, query.Size);
			Assert.Equal(0, query.Offset);
			Assert.Null(query.Filter);
			Assert.NotNull(query.ResultSorts);
			Assert.Single(query.ResultSorts);

			var sort = query.ResultSorts[0];

			var fieldSort = Assert.IsType<FieldResultSort>(sort);
			Assert.Equal("FirstName", fieldSort.FieldName);
			Assert.True(fieldSort.Ascending);

			var fieldRef = Assert.IsType<StringFieldRef>(sort.Field);
			Assert.Equal("FirstName", fieldRef.FieldName);
		}
	}
}
