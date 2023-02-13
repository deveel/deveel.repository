using System;
using System.Linq.Expressions;
using System.Reflection.Metadata.Ecma335;

namespace Deveel.Data {
	public static class FieldMapper {
		public static IEntityFieldMapper<TEntity> Map<TEntity>(Func<string, Expression<Func<TEntity, object>>> mapper)
			where TEntity : class
			=> new DelegatedFieldMapper<TEntity>(mapper);

		class DelegatedFieldMapper<TEntity> : IEntityFieldMapper<TEntity>
			where TEntity : class {
			private readonly Func<string, Expression<Func<TEntity, object>>> mapper;

			public DelegatedFieldMapper(Func<string, Expression<Func<TEntity, object>>> mapper) {
				this.mapper = mapper;
			}

			public Expression<Func<TEntity, object>> Map(string propertyName) {
				return mapper(propertyName);
			}
		}
	}
}
