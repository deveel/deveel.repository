namespace Deveel.Data {
	public static class QueryTests {
		[Fact]
		public static void QueryWithFilterOnly() {
			var query = new Query(QueryFilter.Where<Person>(p => p.FirstName == "John"));

			Assert.NotNull(query.Filter);
			Assert.Null(query.Sort);

			Assert.IsType<ExpressionQueryFilter<Person>>(query.Filter);
		}

		[Fact]
		public static void QueryByWhere() {
			var query = Query.Where<Person>(p => p.FirstName == "John");

			Assert.NotNull(query.Filter);
			Assert.Null(query.Sort);

			Assert.IsType<ExpressionQueryFilter<Person>>(query.Filter);
		}

		[Fact]
		public static void QueryByWhereAndWhere() {
			var query = Query.Where<Person>(p => p.FirstName == "John")
				.And<Person>(p => p.LastName == "Doe");

			Assert.NotNull(query.Filter);
			Assert.Null(query.Sort);

			var combined = Assert.IsType<CombinedQueryFilter>(query.Filter);

			Assert.NotEmpty(combined);
			Assert.Equal(2, combined.Count());

			Assert.IsType<ExpressionQueryFilter<Person>>(combined.ElementAt(0));
			Assert.IsType<ExpressionQueryFilter<Person>>(combined.ElementAt(1));
		}

		[Fact]
		public static void QueryByWhereAndSort() {
			var query = Query.Where<Person>(p => p.FirstName == "John")
				.OrderBy<Person>(p => p.LastName);

			Assert.NotNull(query.Filter);
			Assert.NotNull(query.Sort);

			Assert.IsType<ExpressionQueryFilter<Person>>(query.Filter);
			Assert.IsType<ExpressionSort<Person>>(query.Sort);
		}

		[Fact]
		public static void QueryByWhereAndSortByString() {
			var query = Query.Where<Person>(p => p.FirstName == "John")
				.OrderBy("LastName");

			Assert.NotNull(query.Filter);
			Assert.NotNull(query.Sort);

			Assert.IsType<ExpressionQueryFilter<Person>>(query.Filter);
			Assert.IsType<FieldSort>(query.Sort);
		}

		[Fact]
		public static void QueryByWhereAndSortByStringDescending() {
			var query = Query.Where<Person>(p => p.FirstName == "John")
				.OrderByDescending("LastName");

			Assert.NotNull(query.Filter);
			Assert.NotNull(query.Sort);

			Assert.IsType<ExpressionQueryFilter<Person>>(query.Filter);
			Assert.IsType<FieldSort>(query.Sort);
		}

		[Fact]
		public static void QueryByWhereAndSortDescending() {
			var query = Query.Where<Person>(p => p.FirstName == "John")
				.OrderByDescending<Person>(p => p.LastName);

			Assert.NotNull(query.Filter);
			Assert.NotNull(query.Sort);

			Assert.IsType<ExpressionQueryFilter<Person>>(query.Filter);
			Assert.IsType<ExpressionSort<Person>>(query.Sort);
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
			var query = Query.Where<Person>(p => p.FirstName == "John")
				.OrderBy<Person>(p => p.LastName);

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
