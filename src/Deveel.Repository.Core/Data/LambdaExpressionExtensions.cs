using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

namespace Deveel.Data {
	static class LambdaExpressionExtensions {
		public static Expression<Func<TTarget, bool>> As<TTarget>(this LambdaExpression expression) {
			if (expression.ReturnType != typeof(bool))
				throw new ArgumentException();
			if (expression.Parameters.Count != 1)
				throw new ArgumentException();

			var param = expression.Parameters[0];

			if (!param.Type.IsAssignableFrom(typeof(TTarget)))
				throw new ArgumentException();

			var parameter = Expression.Parameter(typeof(TTarget), param.Name);
			var body = ReplaceType(expression.Body, param.Type, typeof(TTarget));

			return Expression.Lambda<Func<TTarget, bool>>(body, false, parameter);
		}

		private static Expression ReplaceType(Expression expression, Type sourceType, Type destType) {
			var replacer = new TypeReplacer(sourceType, destType);
			return replacer.Visit(expression);
		}

		class TypeReplacer : ExpressionVisitor {
			private readonly Type sourceType;
			private readonly Type destType;

			public TypeReplacer(Type sourceType, Type destType) {
				this.sourceType = sourceType;
				this.destType = destType;
			}

			protected override Expression VisitParameter(ParameterExpression node) {
				if (node.Type == sourceType) {
					node = Expression.Parameter(destType, node.Name);
				}

				return base.VisitParameter(node);
			}

			protected override Expression VisitMember(MemberExpression node) {
				if (node.Member.ReflectedType == sourceType) {
					var newMember = destType.GetMember(node.Member.Name, BindingFlags.Public | BindingFlags.Instance);
					if (newMember.Length > 1)
						throw new AmbiguousMatchException();

					node = Expression.MakeMemberAccess(Visit(node.Expression), newMember[0]);
				}

				return base.VisitMember(node);
			}
		}
	}
}