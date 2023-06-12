using System.Collections;

namespace Various.Collections.Composite.CompositeEnumerator
{
    public class CompositeEnumerator<T> : IEnumerator<T>
    {
        private readonly IEnumerable<T>[] enumerables;

        private int enumeratingCollection = -1;
        private IEnumerator<T>? currentEnumerator;

        public T Current { get; private set; } = default!;
        object IEnumerator.Current => Current!;

        public CompositeEnumerator(params IEnumerable<T>[] enumerables)
        {
            this.enumerables = enumerables;
        }

        public bool MoveNext()
        {
            if (currentEnumerator == null)
            {
                if (++enumeratingCollection < enumerables.Length)
                {
                    currentEnumerator = enumerables[enumeratingCollection].GetEnumerator();
                }
                else
                {
                    return false;
                }
            }

            if (currentEnumerator.MoveNext())
            {
                Current = currentEnumerator.Current;
                return true;
            }
            else
            {
                currentEnumerator.Dispose();
                currentEnumerator = null;
                Current = default!;
                return MoveNext();
            }
        }

        public void Reset()
        {
            Dispose();
            Current = default!;
            enumeratingCollection = -1;
        }

        public void Dispose()
        {
            currentEnumerator?.Dispose();
        }
    }
}
