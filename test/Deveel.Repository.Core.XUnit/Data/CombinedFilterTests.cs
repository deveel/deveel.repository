namespace Deveel.Data {
	public static class CombinedFilterTests {
		[Fact]
		public static void CombineCombinedInstance() {
			var filter1 = new ExpressionQueryFilter<Person>(x => x.FirstName == "John");
			var filter2 = new ExpressionQueryFilter<Person>(x => x.LastName == "Doe");

			var combined1 = QueryFilter.Combine(filter1, filter2);
			var combined2 = combined1.Combine(QueryFilter.Empty);

			Assert.NotNull(combined2);
			var filter = Assert.IsType<CombinedQueryFilter>(combined2);
			Assert.NotNull(filter);
			Assert.Equal(3, filter.Filters.Count);
			Assert.Equal(filter1, filter.Filters[0]);
			Assert.Equal(filter2, filter.Filters[1]);
			Assert.Equal(QueryFilter.Empty, filter.Filters[2]);
		}

		[Fact]
		public static void CombineExpressionFilters() {
			var filter1 = new ExpressionQueryFilter<Person>(x => x.FirstName == "John");
			var filter2 = new ExpressionQueryFilter<Person>(x => x.LastName == "Doe");

			var combined = QueryFilter.Combine(filter1, filter2);

			Assert.NotNull(combined);
			var filter = Assert.IsType<CombinedQueryFilter>(combined);

			Assert.Equal(2, filter.Filters.Count);
			Assert.Equal(filter1, filter.Filters[0]);
			Assert.Equal(filter2, filter.Filters[1]);
		}

		[Fact]
		public static void CombineExpressionFiltersWithEmpty() {
			var filter1 = new ExpressionQueryFilter<Person>(x => x.FirstName == "John");

			var combined = QueryFilter.Combine(filter1, QueryFilter.Empty);

			Assert.NotNull(combined);
			var filter = Assert.IsType<CombinedQueryFilter>(combined);

			Assert.Equal(2, filter.Filters.Count);
			Assert.Equal(filter1, filter.Filters[0]);
			Assert.Equal(QueryFilter.Empty, filter.Filters[1]);
		}

		[Fact]
		public static void ConvertCombinedFilterToLambda() {
			var filter1 = new ExpressionQueryFilter<Person>(x => x.FirstName == "John");
			var filter2 = new ExpressionQueryFilter<Person>(x => x.LastName == "Doe");

			var combined = QueryFilter.Combine(filter1, filter2);

			var lambda = combined.AsLambda<Person>();

			Assert.NotNull(lambda);
			Assert.Equal("x => ((x.FirstName == \"John\") AndAlso (x.LastName == \"Doe\"))", lambda.ToString());
		}

		[Fact]
		public static void ConvertCombinedFilterToLambdaWithEmpty() {
			var filter1 = new ExpressionQueryFilter<Person>(x => x.FirstName == "John");

			var combined = QueryFilter.Combine(filter1, QueryFilter.Empty);

			var lambda = combined.AsLambda<Person>();

			Assert.NotNull(lambda);
			Assert.Equal("x => (x.FirstName == \"John\")", lambda.ToString());
		}

		[Fact]
		public static void CombineEmptyFilters() {
			var combined = QueryFilter.Combine(QueryFilter.Empty, QueryFilter.Empty);

			Assert.NotNull(combined);
			Assert.NotEmpty(combined.Filters);
			Assert.Equal(2, combined.Filters.Count);
		}

		[Fact]
		public static void CombineThreeFilters() {
			var filter1 = new ExpressionQueryFilter<Person>(x => x.FirstName == "John");
			var filter2 = new ExpressionQueryFilter<Person>(x => x.LastName == "Doe");
			var filter3 = new ExpressionQueryFilter<Person>(x => x.Email == "john.doe@example.com");

			var combined = QueryFilter.Combine(filter1, filter2, filter3);
			var lambda = combined.AsLambda<Person>();

			Assert.NotNull(lambda);
			Assert.Equal("x => (((x.FirstName == \"John\") AndAlso (x.LastName == \"Doe\")) AndAlso (x.Email == \"john.doe@example.com\"))", lambda.ToString());
		}
	}
}
