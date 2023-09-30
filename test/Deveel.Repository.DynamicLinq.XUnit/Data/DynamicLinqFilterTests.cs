namespace Deveel.Data {
	public static class DynamicLinqFilterTests {
		[Fact]
		public static void AsLambda() {
			var filter = new DynamicLinqFilter("p", "p.FirstName == \"John\"");

			Assert.Equal("p", filter.ParameterName);
			Assert.Equal("p.FirstName == \"John\"", filter.Expression);

			var expression = filter.AsLambda<Person>();

			Assert.NotNull(expression);
			Assert.Equal(typeof(Person), expression.Parameters[0].Type);
			Assert.Equal(typeof(bool), expression.ReturnType);
		}

		[Fact]
		public static void AsLambda_Invalid() {
			var filter = new DynamicLinqFilter("p", "p.FirstName");

			Assert.Throws<InvalidOperationException>(() => filter.AsLambda<Person>());
		}
	}
}
