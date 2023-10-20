namespace Deveel.Data {
	public static class QueryTests {
		[Fact]
		public static void QueryWithFilterOnly() {
			var query = new Query(QueryFilter.Where<Person>(p => p.FirstName == "John"));

			Assert.NotNull(query.Filter);
			Assert.Null(query.Order);

			Assert.IsType<ExpressionQueryFilter<Person>>(query.Filter);
		}

		[Fact]
		public static void QueryByWhere() {
			var query = Query.Where<Person>(p => p.FirstName == "John");

			Assert.NotNull(query.Filter);
			Assert.Null(query.Order);

			Assert.IsType<ExpressionQueryFilter<Person>>(query.Filter);
		}

		[Fact]
		public static void QueryByWhereAndWhere() {
			var query = new QueryBuilder<Person>()
				.Where(p => p.FirstName == "John")
				.Where(p => p.LastName == "Doe")
				.Query;

			Assert.NotNull(query.Filter);
			Assert.Null(query.Order);

			var combined = Assert.IsType<CombinedQueryFilter>(query.Filter);

			Assert.NotEmpty(combined);
			Assert.Equal(2, combined.Count());

			Assert.IsType<ExpressionQueryFilter<Person>>(combined.ElementAt(0));
			Assert.IsType<ExpressionQueryFilter<Person>>(combined.ElementAt(1));
		}

		[Fact]
		public static void QueryByWhereAndSort() {
			var query = new QueryBuilder<Person>()
				.Where(p => p.FirstName == "John")
				.OrderBy(p => p.LastName)
				.Query;

			Assert.NotNull(query.Filter);
			Assert.NotNull(query.Order);

			Assert.IsType<ExpressionQueryFilter<Person>>(query.Filter);
			Assert.IsType<ExpressionSort<Person>>(query.Order);
		}

		[Fact]
		public static void QueryByWhereAndSortByString() {
			var query = new QueryBuilder<Person>()
				.Where(p => p.FirstName == "John")
				.OrderBy("LastName")
				.Query;

			Assert.NotNull(query.Filter);
			Assert.NotNull(query.Order);

			Assert.IsType<ExpressionQueryFilter<Person>>(query.Filter);
			Assert.IsType<FieldOrder>(query.Order);
		}

		[Fact]
		public static void QueryByWhereAndSortByStringDescending() {
			var query = new QueryBuilder<Person>()
				.Where(p => p.FirstName == "John")
				.OrderByDescending("LastName")
				.Query;

			Assert.NotNull(query.Filter);
			Assert.NotNull(query.Order);

			Assert.IsType<ExpressionQueryFilter<Person>>(query.Filter);
			Assert.IsType<FieldOrder>(query.Order);
		}

		[Fact]
		public static void QueryByWhereAndSortDescending() {
			var query = new QueryBuilder<Person>()
				.Where(p => p.FirstName == "John")
				.OrderByDescending(p => p.LastName)
				.Query;

			Assert.NotNull(query.Filter);
			Assert.NotNull(query.Order);

			Assert.IsType<ExpressionQueryFilter<Person>>(query.Filter);
			Assert.IsType<ExpressionSort<Person>>(query.Order);
		}

		[Fact]
		public static void ApplyQueryByWhere() {
			var query = Query.Where<Person>(p => p.FirstName == "John");

			var people = new List<Person> {
				new Person{ FirstName = "John", LastName = "Doe" },
				new Person{ FirstName = "Jane", LastName = "Doe" },
				new Person{ FirstName = "John", LastName = "Smith" },
				new Person{ FirstName = "Jane", LastName = "Smith" }
			};

			var result = query.Apply(people.AsQueryable());

			Assert.NotNull(result);
			Assert.Equal(2, result.Count());
		}

		[Fact]
		public static void ApplyQueryByWhereAndSort() {
			var query = new QueryBuilder<Person>()
				.Where(p => p.FirstName == "John")
				.OrderBy(p => p.LastName);

			var people = new List<Person> {
				new Person{ FirstName = "John", LastName = "Doe" },
				new Person{ FirstName = "Jane", LastName = "Doe" },
				new Person{ FirstName = "John", LastName = "Smith" },
				new Person{ FirstName = "Jane", LastName = "Smith" }
			};

			var result = query.Apply(people.AsQueryable());

			Assert.NotNull(result);
			Assert.Equal(2, result.Count());
			Assert.Equal("Doe", result.First().LastName);
		}
	}
}
