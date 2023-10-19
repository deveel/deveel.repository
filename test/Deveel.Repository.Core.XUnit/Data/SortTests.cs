namespace Deveel.Data {
	public static class SortTests {
		[Fact]
		public static void CombineSorts() {
			var sort1 = Sort.OrderBy<Person>(x => x.FirstName);
			var sort2 = Sort.OrderByDescending<Person>(x => x.LastName);

			var combined = Sort.Combine(sort1, sort2);

			Assert.NotNull(combined);
			var sort = Assert.IsType<CombinedSort>(combined);
			Assert.NotNull(sort);
			Assert.Equal(2, sort.Count());
			Assert.Equal(sort1, sort.ElementAt(0));
			Assert.True(sort.ElementAt(0).IsAscending());
			Assert.Equal(sort2, sort.ElementAt(1));
			Assert.False(sort.ElementAt(1).IsAscending());
			Assert.True(sort.ElementAt(1).IsDescending());
		}

		[Fact]
		public static void CombineWithCombinedSort() {
			var sort1 = Sort.OrderBy<Person>(x => x.FirstName);
			var sort2 = Sort.OrderByDescending<Person>(x => x.LastName);
			var sort3 = Sort.OrderBy<Person>(x => x.DateOfBirth);

			var combined1 = Sort.Combine(sort1, sort2);
			var combined2 = combined1.Combine(sort3);
		}

		[Fact]
		public static void ApplyCombinedSort() {
			var people = new[] {
				new Person { FirstName = "John", LastName = "Doe" },
				new Person { FirstName = "Jane", LastName = "Doe" },
				new Person { FirstName = "John", LastName = "Smith" },
				new Person { FirstName = "Jane", LastName = "Smith" }
			};

			var queryable = people.AsQueryable();

			var sort1 = Sort.OrderBy<Person>(x => x.FirstName);
			var sort2 = Sort.OrderByDescending<Person>(x => x.LastName);

			var combined = Sort.Combine(sort1, sort2);

			var result = combined.Apply(queryable);

			Assert.NotNull(result);
			var list = result.ToList();
			Assert.Equal(4, list.Count);
		}

		[Fact]
		public static void SortByFieldName_NoMapper() {
			var people = new[] {
				new Person { FirstName = "John", LastName = "Doe" },
				new Person { FirstName = "Jane", LastName = "Doe" },
				new Person { FirstName = "John", LastName = "Smith" },
				new Person { FirstName = "Jane", LastName = "Smith" }
			};

			var queryable = people.AsQueryable();

			var sort = Sort.OrderBy("FirstName");

			var result = sort.Apply(queryable);

			Assert.NotNull(result);
			var list = result.ToList();
			Assert.Equal(4, list.Count);
		}

		[Fact]
		public static void SortByFieldName_DelegatedMapper() {
			var people = new[] {
				new Person { FirstName = "John", LastName = "Doe" },
				new Person { FirstName = "Jane", LastName = "Doe" },
				new Person { FirstName = "John", LastName = "Smith" },
				new Person { FirstName = "Jane", LastName = "Smith" }
			};

			var queryable = people.AsQueryable();

			var sort = Sort.OrderBy("first_name");

			var result = sort.Apply(queryable, field => {
				return field switch {
					"first_name" => p => p.FirstName,
					"last_name" => p => p.LastName,
					_ => throw new ArgumentException($"The field '{field}' is not supported")
				};
			});

			Assert.NotNull(result);

			var list = result.ToList();
			Assert.Equal(4, list.Count);
		}
	}
}
