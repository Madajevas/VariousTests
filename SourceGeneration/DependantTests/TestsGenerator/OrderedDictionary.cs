using Microsoft.CodeAnalysis;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TestsGenerator
{
    internal sealed class OrderedDictionary<TKey, TValue>(IEqualityComparer<TKey> equalityComparer) : IDictionary<TKey, TValue>
    {
        private List<KeyValuePair<TKey, TValue>> items = new();
        private Dictionary<TKey, int> lookup = new(equalityComparer);

        public TValue this[TKey key] {
            get => items[lookup[key]].Value;
            set {
                if (lookup.TryGetValue(key, out var index))
                {
                    items[index] = new(key, value);
                }
                else
                {
                    lookup[key] = items.Count;
                    items.Add(new(key, value));
                }
            }
        }

        public TKey[] Keys => items.Select(i => i.Key).ToArray();
        ICollection<TKey> IDictionary<TKey, TValue>.Keys => Keys;

        public ICollection<TValue> Values => items.Select(i => i.Value).ToArray();

        public int Count => items.Count;

        public bool IsReadOnly => false;

        public void Add(TKey key, TValue value)
        {
            lookup.Add(key, items.Count);
            items.Add(new(key, value));
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            lookup.Add(item.Key, items.Count);
            items.Add(item);
        }

        public void Clear()
        {
            items.Clear();
            lookup.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item) => lookup.ContainsKey(item.Key);

        public bool ContainsKey(TKey key) => lookup.ContainsKey(key);

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => throw new NotImplementedException();

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => items.GetEnumerator();

        public bool Remove(TKey key)
        {
            if (lookup.TryGetValue(key, out var index))
            {
                items.RemoveAt(index);
                lookup.Remove(key);
                foreach (var lookupEntry in lookup)
                {
                    if (lookupEntry.Value > index)
                    {
                        lookup[lookupEntry.Key]--;
                    }
                }

                return true;
            }

            return false;
        }

        public bool Remove(KeyValuePair<TKey, TValue> item) => Remove(item.Key);

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (lookup.TryGetValue(key, out var index))
            {
                value = items[index].Value;
                return true;
            }

            value = default(TValue);
            return false;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
