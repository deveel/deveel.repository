using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

using MongoDB.Bson;
using MongoDB.Driver;

namespace Deveel.Repository {
	static class LambdaExpressionExtensions {
		public static FilterDefinition<TEntity>? AsMongoFilter<TEntity>(this LambdaExpression expression) {
			return Convert<TEntity>(expression.Body);
		}

		private static FilterDefinition<TEntity>? Convert<TEntity>(Expression expression) {
			var converter = new MongoDbFilterConverter<TEntity>();
			converter.Visit(expression);

			return converter.Filter;
		}

		private static string GetFieldName(Expression expression) {
			//if (expression.NodeType == ExpressionType.Parameter)
			//	return ((ParameterExpression)expression).Name;
			if (expression.NodeType == ExpressionType.MemberAccess)
				return ((MemberExpression)expression).Member.Name;

			throw new NotSupportedException($"Cannot determine the field name from an expression of type '{expression.NodeType}'");
		}

		private static object? GetConstantValue(Expression? expression) {
			if (expression == null)
				return null;

			if (expression.CanReduce)
				expression = expression.Reduce();

			if (expression is MemberExpression memberExp) {
				var obj = GetConstantValue(memberExp.Expression);

				if (memberExp.Member is PropertyInfo property) {
					return property.GetValue(obj);
				} else if (memberExp.Member is FieldInfo field) {
					return field.GetValue(obj);
				} else {
					throw new NotSupportedException();
				}
			}

			if (expression.NodeType != ExpressionType.Constant) {
				throw new NotSupportedException($"Cannot determine the comparison value from an expression of type '{expression.NodeType}'");
			}

			return ((ConstantExpression)expression).Value;
		}

		class MongoDbFilterConverter<TEntity> : ExpressionVisitor {
			public FilterDefinition<TEntity>? Filter { get; private set; }

			[return: NotNullIfNotNull("node")]
			public override Expression? Visit(Expression? node) {
				if (node == null)
					return null;

				switch (node.NodeType) {
					case ExpressionType.And:
					case ExpressionType.AndAlso:
					case ExpressionType.Equal:
					case ExpressionType.GreaterThan:
					case ExpressionType.GreaterThanOrEqual:
					case ExpressionType.LessThan:
					case ExpressionType.LessThanOrEqual:
					case ExpressionType.Negate:
					case ExpressionType.Not:
					case ExpressionType.NotEqual:
					case ExpressionType.Or:
					case ExpressionType.OrElse:
					case ExpressionType.IsTrue:
					case ExpressionType.IsFalse:
					case ExpressionType.MemberAccess:
					case ExpressionType.Call:
						return base.Visit(node);
				}

				throw new NotSupportedException($"Expression of type '{node.NodeType}' is not supported");
			}

			protected override Expression VisitMethodCall(MethodCallExpression node) {
				if (node.Arguments.Count != 1)
					throw new ArgumentException();

				if (node.Method.Name == "Equals") {
					Filter = Builders<TEntity>.Filter.Eq(GetFieldName(node.Object), GetConstantValue(node.Arguments[0]));
					return node;
				}

				if (node.Method.ReflectedType == typeof(string)) {
					if (!(GetConstantValue(node.Arguments[0]) is string s))
						throw new ArgumentException();

					if (node.Method.Name == nameof(string.Contains)) {
						Filter = Builders<TEntity>.Filter.AnyEq(GetFieldName(node.Object), s);
						return node;
					} else if (node.Method.Name == nameof(string.StartsWith)) {
						Filter = Builders<TEntity>.Filter.Regex(GetFieldName(node.Object), $"^{s}.*");
						return node;
					} else if (node.Method.Name == nameof(string.EndsWith)) {
						Filter = Builders<TEntity>.Filter.Regex(GetFieldName(node.Object), $"{s}$");
						return node;
					} else {
						throw new NotSupportedException();
					}
				}

				return base.VisitMethodCall(node);
			}

			protected override Expression VisitMember(MemberExpression node) {
				if (node.Member is PropertyInfo property) {
					if (property.PropertyType != typeof(bool))
						throw new NotSupportedException();

					Filter = Builders<TEntity>.Filter.Eq(property.Name, true);
					return node;
				} else if (node.Member is FieldInfo field) {
					if (field.FieldType != typeof(bool))
						throw new NotSupportedException();

					Filter = Builders<TEntity>.Filter.Eq(field.Name, true);
					return node;
				}

				return base.VisitMember(node);
			}

			protected override Expression VisitUnary(UnaryExpression node) {
				if (node.NodeType == ExpressionType.Not) {
					if (node.Operand is MemberExpression memberExpr) {
						if (memberExpr.Member is PropertyInfo property && property.PropertyType == typeof(bool)) {
							Filter = Builders<TEntity>.Filter.Eq(memberExpr.Member.Name, false);
							return node;
						} else if (memberExpr.Member is FieldInfo field && field.FieldType == typeof(bool)) {
							Filter = Builders<TEntity>.Filter.Eq(memberExpr.Member.Name, false);
							return node;
						} else {
							throw new NotSupportedException();
						}
					} else {
						var operand = Convert<TEntity>(node.Operand);
						Filter = Builders<TEntity>.Filter.Not(operand);
					}
				}

				return base.VisitUnary(node);
			}



			protected override Expression VisitBinary(BinaryExpression node) {
				switch (node.NodeType) {
					case ExpressionType.And:
					case ExpressionType.AndAlso: {
							var left = Convert<TEntity>(node.Left);
							var right = Convert<TEntity>(node.Right);

							Filter = Builders<TEntity>.Filter.And(left, right);
							break;
						}
					case ExpressionType.Equal:
						Filter = Builders<TEntity>.Filter.Eq(GetFieldName(node.Left), GetConstantValue(node.Right));
						break;
					case ExpressionType.GreaterThan:
						Filter = Builders<TEntity>.Filter.Gt(GetFieldName(node.Left), GetConstantValue(node.Right));
						break;
					case ExpressionType.GreaterThanOrEqual:
						Filter = Builders<TEntity>.Filter.Gte(GetFieldName(node.Left), GetConstantValue(node.Right));
						break;
					case ExpressionType.LessThan:
						Filter = Builders<TEntity>.Filter.Lt(GetFieldName(node.Left), GetConstantValue(node.Right));
						break;
					case ExpressionType.LessThanOrEqual:
						Filter = Builders<TEntity>.Filter.Lte(GetFieldName(node.Left), GetConstantValue(node.Right));
						break;
					case ExpressionType.NotEqual:
						Filter = Builders<TEntity>.Filter.Ne(GetFieldName(node.Left), GetConstantValue(node.Right));
						break;
					case ExpressionType.Or:
					case ExpressionType.OrElse: {
							var left = Convert<TEntity>(node.Left);
							var right = Convert<TEntity>(node.Right);

							Filter =  Builders<TEntity>.Filter.Or(left, right);
							break;
						}
					case ExpressionType.IsTrue:
						Filter = Builders<TEntity>.Filter.Eq(GetFieldName(node.Left), true);
						break;
					case ExpressionType.IsFalse:
						Filter = Builders<TEntity>.Filter.Eq(GetFieldName(node.Left), false);
						break;
					default:
						throw new NotSupportedException($"The expression type '{node.NodeType}' is not supported");
				}

				return node;
			}
		}
	}
}
