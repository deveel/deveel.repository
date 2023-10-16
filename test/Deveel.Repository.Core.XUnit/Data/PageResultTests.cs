namespace Deveel.Data {
	public static class PageResultTests {
		[Fact]
		public static void FirstPageOfMany() {
			var faker = new PersonFaker();
			var query = new PageQuery<Person>(1, 10);
			var items = faker.Generate(10);
			var result = new PageResult<Person>(query, 100, items);

			Assert.NotNull(result);
			Assert.Equal(100, result.TotalItems);
			Assert.Equal(10, result.TotalPages);
			Assert.Equal(1, result.Request.Page);
			Assert.Equal(10, result.Request.Size);
			Assert.NotNull(result.Items);
			Assert.Equal(10, result.Items.Count);
			Assert.True(result.IsFirstPage);
			Assert.False(result.IsLastPage);
			Assert.True(result.HasNextPage);
			Assert.False(result.HasPreviousPage);
			Assert.Equal(2, result.NextPage);
			Assert.Null(result.PreviousPage);
		}

		[Fact]
		public static void LastPageOfMany() {
			var faker = new PersonFaker();
			var query = new PageQuery<Person>(10, 10);
			var items = faker.Generate(8);
			var result = new PageResult<Person>(query, 100, items);

			Assert.NotNull(result);
			Assert.Equal(100, result.TotalItems);
			Assert.Equal(10, result.TotalPages);
			Assert.Equal(10, result.Request.Page);
			Assert.Equal(10, result.Request.Size);
			Assert.NotNull(result.Items);
			Assert.Equal(8, result.Items.Count);
			Assert.False(result.IsFirstPage);
			Assert.True(result.IsLastPage);
			Assert.False(result.HasNextPage);
			Assert.True(result.HasPreviousPage);
			Assert.Null(result.NextPage);
			Assert.Equal(9, result.PreviousPage);
		}

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
