using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Deveel.Data {
	public static class FilterExpression {
		#region AsExpression

		public static Expression<Func<T, bool>> AsLambda<T>(string paramName, string expression) {
			var paramExp = new[] { Expression.Parameter(typeof(T), paramName) };
			var exp = DynamicExpressionParser.ParseLambda(ParsingConfig.Default, paramExp, typeof(bool), expression);

			if (exp.ReturnType != typeof(bool))
				throw new InvalidOperationException("The resulting expression is not a filter");

			return (Expression<Func<T, bool>>)exp;
		}

		public static Expression<Func<object, bool>> AsLambda(Type type, string paramName, string expression) {
			var paramExp = new[] { Expression.Parameter(type, paramName) };
			var exp = DynamicExpressionParser.ParseLambda(ParsingConfig.Default, paramExp, typeof(bool), expression);

			if (exp.ReturnType != typeof(bool))
				throw new InvalidOperationException("The resulting expression is not a filter");

			return (Expression<Func<object, bool>>)exp;
		}

		#endregion

		#region Compile

		public static Delegate Compile(Type[] paramTypes, string[] paramNames, string expression)
			=> Compile(null, paramTypes, paramNames, expression);

		public static Delegate Compile(IFilterCache cache, Type[] paramTypes, string[] paramNames, string expression) {
			if (paramTypes is null)
				throw new ArgumentNullException(nameof(paramTypes));
			if (paramNames is null)
				throw new ArgumentNullException(nameof(paramNames));
			if (string.IsNullOrWhiteSpace(expression))
				throw new ArgumentException($"'{nameof(expression)}' cannot be null or whitespace.", nameof(expression));

			var func = cache?.Get(expression);

			if (func == null) {
				if (paramTypes.Length != paramNames.Length)
					throw new ArgumentException("The types and the names arrays are not the same size");

				var parameters = new ParameterExpression[paramTypes.Length];
				for (int i = 0; i < paramTypes.Length; i++) {
					var paramType = paramTypes[i];
					var paramName = paramNames[i];

					parameters[i] = Expression.Parameter(paramType, paramName);
				}

				var exp = DynamicExpressionParser.ParseLambda(ParsingConfig.Default, parameters, typeof(bool), expression);

				if (exp.ReturnType != typeof(bool))
					throw new InvalidOperationException("The resulting expression is not a filter");

				func = exp.Compile();
				cache?.Set(expression, func);
			}

			return func;
		}

		public static Delegate Compile(IFilterCache cache, Type paramType, string paramName, string expression)
			=> Compile(cache, new Type[] { paramType }, new string[] { paramName }, expression);

		public static Delegate Compile(Type paramType, string paramName, string expression)
			=> Compile(null, paramType, paramName, expression);

		public static Func<T, bool> Compile<T>(IFilterCache cache, string paramName, string expression)
			=> (Func<T, bool>)Compile(cache, typeof(T), paramName, expression);

		public static Func<T, bool> Compile<T>(string paramName, string expression)
			=> Compile<T>(null, paramName, expression);

		#endregion

		public static bool IsValid(Type paramType, string paramName, string expression)
			=> IsValid(new Type[] { paramType }, new string[] { paramName }, expression);

		public static bool IsValid(Type[] paramTypes, string[] paramNames, string expression) {
			if (paramTypes.Length != paramNames.Length)
				throw new ArgumentException("The types and the names arrays are not the same size");

			var parameters = new ParameterExpression[paramTypes.Length];
			for (int i = 0; i < paramTypes.Length; i++) {
				var paramType = paramTypes[i];
				var paramName = paramNames[i];

				parameters[i] = Expression.Parameter(paramType, paramName);
			}

			try {
				var exp = DynamicExpressionParser.ParseLambda(ParsingConfig.Default, parameters, typeof(bool), expression);

				if (exp.ReturnType != typeof(bool))
					return false;

				var func = exp.Compile();

				return func != null;
			} catch (Exception) {
				// TODO: try to interpret the error
				return false;
			}
		}

		public static bool Evaluate(Type paramType, string paramName, string expression, object obj) {
			if (paramType is null)
				throw new ArgumentNullException(nameof(paramType));
			if (obj is null)
				throw new ArgumentNullException(nameof(obj));

			if (!paramType.IsInstanceOfType(obj))
				throw new ArgumentException($"The object is not of type {paramType}", nameof(obj));

			return (bool)Compile(paramType, paramName, expression).DynamicInvoke(obj);
		}

		public static bool Evaluate<T>(string paramName, string expression, T obj)
			=> Evaluate(typeof(T), paramName, expression, obj);
	}
}
