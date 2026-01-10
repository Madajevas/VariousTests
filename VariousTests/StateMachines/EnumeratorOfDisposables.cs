using System.Collections;
using System.ComponentModel;

namespace VariousTests.StateMachines
{
    internal class EnumeratorOfDisposables : IEnumerable<IDisposable>, IEnumerator<IDisposable>
    {
        private const short INITIAL = 0;
        private const short SECOND = 1;
        private const short CLEAN  = 2;

        private short state;

        public EnumeratorOfDisposables() : this(-1) { }

        private EnumeratorOfDisposables(short state)
        {
            this.state = state;
        }

        public IDisposable Current { get; private set; }

        object IEnumerator.Current => Current;

        public IEnumerator<IDisposable> GetEnumerator()
        {
            return new EnumeratorOfDisposables(INITIAL);
        }

        public bool MoveNext()
        {
            switch (state)
            {
                case INITIAL:
                    Current = Substitute.For<IDisposable>();
                    state = SECOND;
                    return true;
                case SECOND:
                    Current.Dispose();
                    Current = Substitute.For<IDisposable>();
                    state = CLEAN;
                    return true;
                case CLEAN:
                    Current.Dispose();
                    return false;
                default:
                    return false;
            }
        }

        public void Dispose()
        {
            Current?.Dispose();
        }

        public void Reset() => throw new NotSupportedException();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
