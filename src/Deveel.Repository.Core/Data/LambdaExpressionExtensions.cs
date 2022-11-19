using System;
using System.Linq.Expressions;

namespace Deveel.Data
{
    static class LambdaExpressionExtensions
    {
        public static Expression<Func<TFacade, bool>> Cast<TFacade>(this LambdaExpression expression)
        {
            var parameter = Expression.Parameter(typeof(TFacade), "f");
            return Expression.Lambda<Func<TFacade, bool>>(expression, parameter);
        }
    }
}
