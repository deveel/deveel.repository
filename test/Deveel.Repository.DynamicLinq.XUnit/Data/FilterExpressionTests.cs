namespace Deveel.Data {
	public static class FilterExpressionTests {
		[Fact]
		public static void AsLambda() {
			var expression = FilterExpression.AsLambda<Person>("p", "p.FirstName == \"John\"");

			Assert.NotNull(expression);
			Assert.Equal(typeof(Person), expression.Parameters[0].Type);
			Assert.Equal(typeof(bool), expression.ReturnType);
		}

		[Fact]
		public static void AsLambda_Invalid() {
			Assert.Throws<InvalidOperationException>(() => FilterExpression.AsLambda<Person>("p", "p.FirstName"));
		}

		[Fact]
		public static void AsLambda_InvalidType() {
			Assert.Throws<InvalidOperationException>(() => FilterExpression.AsLambda<Person>("p", "p.FirstName == 1"));
		}

		[Fact]
		public static void CompileWithSingleParameter() {
			var result = FilterExpression.Compile(typeof(Person), "p", "p.FirstName == \"John\"");

			Assert.NotNull(result);

			var func = Assert.IsType<Func<Person, bool>>(result);

			Assert.NotNull(func);
			Assert.NotNull(func.Method);

			var pars = func.Method.GetParameters();
			Assert.NotNull(pars);
			Assert.NotEmpty(pars);
			Assert.Equal(2, pars.Length);
			Assert.Equal(typeof(Person), pars[1].ParameterType);
			Assert.Equal(typeof(bool), func.Method.ReturnType);
		}

		[Fact]
		public static void CompileWithSingleParameter_Cached() {
			var cache = new SimpleFilterCache();

			var result = FilterExpression.Compile(cache, typeof(Person), "p", "p.FirstName == \"John\"");

			Assert.NotNull(result);

			var func = Assert.IsType<Func<Person, bool>>(result);

			Assert.NotNull(func);
			Assert.NotNull(func.Method);

			var pars = func.Method.GetParameters();
			Assert.NotNull(pars);
			Assert.NotEmpty(pars);
			Assert.Equal(2, pars.Length);
			Assert.Equal(typeof(Person), pars[1].ParameterType);
			Assert.Equal(typeof(bool), func.Method.ReturnType);

			Assert.True(cache.TryGet("p.FirstName == \"John\"", out var cachedFunc));
			Assert.NotNull(cachedFunc);
			Assert.Equal(func, cachedFunc);
		}

		[Fact]
		public static void CompileWithSingleParameter_Typed() {
			var result = FilterExpression.Compile<Person>("p", "p.FirstName == \"John\"");

			Assert.NotNull(result);

			var func = Assert.IsType<Func<Person, bool>>(result);

			Assert.NotNull(func);
			Assert.NotNull(func.Method);

			var pars = func.Method.GetParameters();
			Assert.NotNull(pars);
			Assert.NotEmpty(pars);
			Assert.Equal(2, pars.Length);
			Assert.Equal(typeof(Person), pars[1].ParameterType);
			Assert.Equal(typeof(bool), func.Method.ReturnType);
		}

		class SimpleFilterCache : IFilterCache {
			private readonly Dictionary<string, Delegate> cache;

			public SimpleFilterCache() {
				cache = new Dictionary<string, Delegate>();
			}

			public void Set(string expression, Delegate func) {
				cache[expression] = func;
			}

			public bool TryGet(string expression, out Delegate? lambda) {
				return cache.TryGetValue(expression, out lambda);
			}
		}
	}
}
