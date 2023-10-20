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
			Assert.Equal(3, filter.Count());
			Assert.Equal(filter1, filter.ElementAt(0));
			Assert.Equal(filter2, filter.ElementAt(1));
			Assert.Equal(QueryFilter.Empty, filter.ElementAt(2));
		}

		[Fact]
		public static void CombineExpressionFilters() {
			var filter1 = new ExpressionQueryFilter<Person>(x => x.FirstName == "John");
			var filter2 = new ExpressionQueryFilter<Person>(x => x.LastName == "Doe");

			var combined = QueryFilter.Combine(filter1, filter2);

			Assert.NotNull(combined);
			var filter = Assert.IsType<CombinedQueryFilter>(combined);

			Assert.Equal(2, filter.Count());
			Assert.Equal(filter1, filter.ElementAt(0));
			Assert.Equal(filter2, filter.ElementAt(1));
		}

		[Fact]
		public static void CombineExpressionFiltersWithEmpty() {
			var filter1 = new ExpressionQueryFilter<Person>(x => x.FirstName == "John");

			var combined = QueryFilter.Combine(filter1, QueryFilter.Empty);

			Assert.NotNull(combined);
			var filter = Assert.IsType<CombinedQueryFilter>(combined);

			Assert.Single(filter);
			Assert.Equal(filter1, filter.ElementAt(0));
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
			Assert.Throws<ArgumentException>(() => QueryFilter.Combine(QueryFilter.Empty, QueryFilter.Empty));
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
