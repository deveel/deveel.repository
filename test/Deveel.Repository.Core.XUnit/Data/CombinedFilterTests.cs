using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deveel.Data {
	public static class CombinedFilterTests {
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
	}
}
