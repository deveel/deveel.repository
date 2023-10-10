namespace Deveel.Data {
	public static class PageResultTests {
		[Fact]
		public static void NewPage_InvalidTotal() {
			Assert.Throws<ArgumentOutOfRangeException>(() => new PageResult<Person>(new PageQuery<Person>(1, 10), -1));
		}

		[Fact]
		public static void NewPage_NullRequest() {
			Assert.Throws<ArgumentNullException>(() => new PageResult<Person>(null!, 0));
		}

		[Fact]
		public static void EmptyResult() {
			var result = PageResult<Person>.Empty(new PageQuery<Person>(1, 10));

			Assert.NotNull(result);
			Assert.Equal(0, result.TotalItems);
			Assert.Equal(0, result.TotalPages);
			Assert.Equal(1, result.Request.Page);
			Assert.Equal(10, result.Request.Size);
		}
	}
}
