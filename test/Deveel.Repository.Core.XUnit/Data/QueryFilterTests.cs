﻿namespace Deveel.Data {
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
		public static void AsLambda_DifferentTarget() {
			var expr = new ExpressionQueryFilter<Person>(x => x.FirstName == "John");

			Assert.Throws<ArgumentException>(() => expr.AsLambda<Company>());
		}

		[Fact]
		public static void AsLambda_DifferentParameter() {
			var expr = new ExpressionQueryFilter<Person>(x => x.FirstName == "John");

			var result = expr.AsLambda<Employee>();

			Assert.NotNull(result);
			Assert.Equal("x => (x.FirstName == \"John\")", result.ToString());
		}

		[Fact]
		public static void IsEmpty() {
			Assert.True(QueryFilter.Empty.IsEmpty());
			Assert.False(new ExpressionQueryFilter<Person>(x => x.FirstName == "John").IsEmpty());
		}


		[Fact]
		public static void Queryable_FirstOrDefault() {
			var list = new PersonFaker().Generate(10);
			var person = list[Random.Shared.Next(0, 9)];

			var filter = new ExpressionQueryFilter<Person>(x => x.FirstName == person.FirstName);
			var result = list.AsQueryable().FirstOrDefault(filter);

			Assert.NotNull(result);
			Assert.Equal(person.FirstName, result.FirstName);
		}

		[Fact]
		public static void Queryable_ToList() {
			var list = new PersonFaker().Generate(100);
			var person = list[Random.Shared.Next(0, 99)];

			var expected = list.Where(x => x.FirstName == person.FirstName).ToList();

			var filter = new ExpressionQueryFilter<Person>(x => x.FirstName == person.FirstName);
			var result = list.AsQueryable().ToList(filter);

			Assert.NotNull(result);
			Assert.Equal(expected.Count, result.Count);
			Assert.Equal(expected.First().FirstName, result.First().FirstName);
		}

		[Fact]
		public static void Any() {
			var list = new PersonFaker().Generate(100);
			var person = list[Random.Shared.Next(0, 99)];

			var filter = new ExpressionQueryFilter<Person>(x => x.FirstName == person.FirstName);
			var result = list.AsQueryable().Any(filter);

			Assert.True(result);
		}

		class Company {
			public string Name { get; set; }

			public string Address { get; set; }
		}

		class Employee : Person {
			public string CompanyName { get; set; }

			public string EmployeeNumber { get; set; }
		}
	}
}
