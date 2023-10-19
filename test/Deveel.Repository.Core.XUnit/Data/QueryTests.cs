namespace Deveel.Data {
	public static class QueryTests {
		[Fact]
		public static void QueryWithFilterOnly() {
			var query = new Query(QueryFilter.Where<Person>(p => p.FirstName == "John"));

			Assert.NotNull(query.Filter);
			Assert.Null(query.Sort);

			Assert.IsType<ExpressionQueryFilter<Person>>(query.Filter);
		}
	}
}
