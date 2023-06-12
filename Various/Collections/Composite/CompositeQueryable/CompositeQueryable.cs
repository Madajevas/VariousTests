using System.Collections;
using System.Linq.Expressions;

namespace Various.Collections.Composite.CompositeQueryable
{
    public class CompositeQueryable<T> : IQueryable<T>
    {
        private readonly IQueryable<T>[] innerQueries;

        public Type ElementType => typeof(T);
        public Expression Expression => Expression.Constant(this);
        public IQueryProvider Provider => new CompositeQueryProvider<T>(innerQueries);

        public CompositeQueryable(params IQueryable<T>[] innerQueries)
        {
            this.innerQueries = innerQueries;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return innerQueries.SelectMany(q => q).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
