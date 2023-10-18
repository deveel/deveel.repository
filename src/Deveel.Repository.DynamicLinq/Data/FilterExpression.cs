﻿// Copyright 2023 Deveel AS
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

namespace Deveel.Data {
	/// <summary>
	/// Provides helpers to create and evaluate filter expressions
	/// </summary>
	public static class FilterExpression {
		/// <summary>
		/// Converts the given expression string into a <see cref="LambdaExpression"/>
		/// to be used as a filter of a <see cref="IQueryable{T}"/>
		/// </summary>
		/// <typeparam name="T">
		/// The type of the object to be filtered
		/// </typeparam>
		/// <param name="paramName">
		/// The name of the parameter to be used in the expression
		/// </param>
		/// <param name="expression">
		/// The LINQ expression string to be parsed
		/// </param>
		/// <returns>
		/// Returns a <see cref="LambdaExpression"/> that can be used as a filter
		/// to a <see cref="IQueryable{T}"/>.
		/// </returns>
		/// <exception cref="InvalidOperationException">
		/// Thrown when the resulting expression is not a filter (the 
		/// result type is not <see cref="bool"/>).
		/// </exception>
		public static Expression<Func<T, bool>> AsLambda<T>(string paramName, string expression) {
			try {
				var paramExp = new[] { Expression.Parameter(typeof(T), paramName) };
				var exp = DynamicExpressionParser.ParseLambda(ParsingConfig.Default, paramExp, typeof(bool), expression);

				if (exp.ReturnType != typeof(bool))
					throw new InvalidOperationException("The resulting expression is not a filter");

				return (Expression<Func<T, bool>>)exp;
			} catch (Exception ex) {
				throw new InvalidOperationException("Could not create the lambda expression", ex);
			}
		}

		#region Compile

		/// <summary>
		/// Converts the given expression string into a function
		/// that can be used to filter a collection of objects
		/// </summary>
		/// <param name="paramTypes">
		/// The array of types of the parameters to be used in the expression
		/// </param>
		/// <param name="paramNames">
		/// The array of names of the parameters to be used in the expression
		/// </param>
		/// <param name="expression"></param>
		/// <returns></returns>
		public static Delegate Compile(Type[] paramTypes, string[] paramNames, string expression)
			=> Compile(null, paramTypes, paramNames, expression);

		/// <summary>
		/// Compiles the given expression string into a function
		/// that can be used to filter a collection of objects.
		/// </summary>
		/// <param name="cache">
		/// An object that provides a cache for the compiled functions,
		/// to avoid recompilation of the same expression.
		/// </param>
		/// <param name="paramTypes">
		/// The array of types of the parameters to be used in the expressios.
		/// </param>
		/// <param name="paramNames">
		/// The array of names of the parameters to be used in the expression.
		/// </param>
		/// <param name="expression">
		/// The expression string to be compiled.
		/// </param>
		/// <returns>
		/// Returns a <see cref="Delegate"/> that can be used to filter a collection
		/// of objects.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown when the given <paramref name="paramTypes"/> or <paramref name="paramNames"/>
		/// are <c>null</c>.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Thrown when the given <paramref name="expression"/> is <c>null</c> 
		/// or whitespace, or when the given <paramref name="paramTypes"/> and
		/// <paramref name="paramNames"/> arrays are not the same size.
		/// </exception>
		/// <exception cref="InvalidOperationException"></exception>
		public static Delegate Compile(IFilterCache? cache, Type[] paramTypes, string[] paramNames, string expression) {
			ArgumentNullException.ThrowIfNull(paramTypes, nameof(paramTypes));
			ArgumentNullException.ThrowIfNull(paramNames, nameof(paramNames));

			if (string.IsNullOrWhiteSpace(expression))
				throw new ArgumentException($"'{nameof(expression)}' cannot be null or whitespace.", nameof(expression));

			if (cache == null || !cache.TryGet(expression, out var func)) {
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

			if (func == null)
				throw new InvalidOperationException("Could not compile the expression");

			return func;
		}

		/// <summary>
		/// Compiles the given expression string into a function
		/// that can be executed to filter a collection of objects.
		/// </summary>
		/// <param name="cache">
		/// A cache object that can be used to avoid recompilation of the same
		/// expression string.
		/// </param>
		/// <param name="paramType">
		/// The type of the parameter to be used in the expression.
		/// </param>
		/// <param name="paramName">
		/// The name of the parameter to be used in the expression.
		/// </param>
		/// <param name="expression">
		/// The expression string to be compiled.
		/// </param>
		/// <returns>
		/// Returns a <see cref="Delegate"/> that can be used to filter a collection
		/// of objects.
		/// </returns>
		/// <seealso cref="Compile(IFilterCache?, Type[], string[], string)"/>
		public static Delegate Compile(IFilterCache? cache, Type paramType, string paramName, string expression)
			=> Compile(cache, new Type[] { paramType }, new string[] { paramName }, expression);

		/// <summary>
		/// Compiles the given expression string into a function
		/// that can be executed to filter a collection of objects.
		/// </summary>
		/// <param name="paramType">
		/// The type of the parameter to be used in the expression.
		/// </param>
		/// <param name="paramName">
		/// The name of the parameter to be used in the expression.
		/// </param>
		/// <param name="expression">
		/// The expression string to be compiled.
		/// </param>
		/// <returns>
		/// Returns a <see cref="Delegate"/> that can be used to filter a collection
		/// of objects.
		/// </returns>
		/// <seealso cref="Compile(IFilterCache?, Type[], string[], string)"/>
		/// <seealso cref="Compile(IFilterCache?, Type, string, string)"/>
		public static Delegate Compile(Type paramType, string paramName, string expression)
			=> Compile(null, paramType, paramName, expression);

		/// <summary>
		/// Compiles the given expression string into a function
		/// that can be executed to filter a collection of objects.
		/// </summary>
		/// <typeparam name="T">
		/// The type of the parameter to be used in the expression.
		/// </typeparam>
		/// <param name="cache">
		/// A cache object that can be used to avoid recompilation of the same
		/// expression string.
		/// </param>
		/// <param name="paramName">
		/// The name of the parameter to be used in the expression.
		/// </param>
		/// <param name="expression">
		/// The expression string to be compiled.
		/// </param>
		/// <returns>
		/// Returns a <see cref="Delegate"/> that can be used to filter a collection
		/// of objects.
		/// </returns>
		/// <seealso cref="Compile(IFilterCache?, Type, string, string)"/>
		public static Func<T, bool> Compile<T>(IFilterCache? cache, string paramName, string expression)
			=> (Func<T, bool>)Compile(cache, typeof(T), paramName, expression);

		/// <summary>
		/// Compiles the given expression string into a function
		/// that can be executed to filter a collection of objects.
		/// </summary>
		/// <typeparam name="T">
		/// The type of the parameter to be used in the expression.
		/// </typeparam>
		/// <param name="cache">
		/// A cache object that can be used to avoid recompilation of the same
		/// expression string.
		/// </param>
		/// <param name="paramName">
		/// The name of the parameter to be used in the expression.
		/// </param>
		/// <param name="expression">
		/// The expression string to be compiled.
		/// </param>
		/// <returns>
		/// Returns a <see cref="Delegate"/> that can be used to filter a collection
		/// of objects.
		/// </returns>
		/// <seealso cref="Compile(IFilterCache?, Type, string, string)"/>
		public static Func<T, bool> Compile<T>(string paramName, string expression)
			=> Compile<T>(null, paramName, expression);

		#endregion
	}
}
