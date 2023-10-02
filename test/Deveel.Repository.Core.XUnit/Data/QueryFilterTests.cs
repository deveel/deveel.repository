namespace Deveel.Data {
	public static class QueryFilterTests {
		[Fact]
		public static void AsLambda_FromExpression() {
			var expr = new ExpressionQueryFilter<Person>(x => x.FirstName == "John");

			var lambda = expr.AsLambda<Person>();

			Assert.NotNull(lambda);
			Assert.Equal("x => (x.FirstName == \"John\")", lambda.ToString());
		}

		[Fact]
		public static void AsLambda_FromEmpty() {
			var lambda = QueryFilter.Empty.AsLambda<Person>();

			Assert.NotNull(lambda);
			Assert.Equal("x => True", lambda.ToString());
		}

		[Fact]
		public static void IsEmpty() {
			Assert.True(QueryFilter.Empty.IsEmpty());
			Assert.False(new ExpressionQueryFilter<Person>(x => x.FirstName == "John").IsEmpty());
		}
	}
}
