using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Various.Collections.Composite.CompositeQueryable;

internal class CompositeQueryProvider<T> : IQueryProvider
{
    private readonly IQueryable<T>[] innerQueries;

    public CompositeQueryProvider(IQueryable<T>[] innerQueries)
    {
        this.innerQueries = innerQueries;
    }

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
    {
        var queries = GetQueryables<TElement>(expression).ToArray();

        return new CompositeQueryable<TElement>(queries);
    }

    private IEnumerable<IQueryable<TElement>> GetQueryables<TElement>(Expression expression)
    {
        if (expression is not MethodCallExpression methodCallExpression)
        {
            throw new NotImplementedException();
        }
        var arguments = methodCallExpression.Arguments;

        IEnumerable<Expression> GetArguments(IQueryable<T> inner)
        {
            for (var i = 0; i < arguments.Count; i++)
            {
                var arg = arguments[i];

                if (arg is ConstantExpression constExpr && constExpr.Type == typeof(CompositeQueryable<T>))
                {
                    yield return Expression.Constant(inner);
                    continue;
                }

                yield return arg;
            }
        }

        foreach (var query in innerQueries)
        {
            var argsDebug = GetArguments(query).ToList();
            var body = Expression.Call(null, methodCallExpression.Method, argsDebug);

            var newQueryable = (IQueryable<TElement>)query.Provider.Execute(body);

            yield return newQueryable;
        }
    }

    public IQueryable CreateQuery(Expression expression)
    {
        throw new NotImplementedException();
    }

    public object? Execute(Expression expression)
    {
        throw new NotImplementedException();
    }

    public TResult Execute<TResult>(Expression expression)
    {
        throw new NotImplementedException();
    }
}
