using Microsoft.AspNetCore.WebUtilities;

using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Web;

namespace Various.Parsing
{
    public interface IBackplaneStrategy
    {
        static abstract IBackplaneStrategy Create(string formData);

        bool TryGetValue(string key, out ReadOnlySpan<char> value);
    }

    public class StringToStringDictionaryBackplaneStrategy(IReadOnlyDictionary<string, string> form) : IBackplaneStrategy
    {
        public static IBackplaneStrategy Create(string formData)
        {
            using var reader = new FormReader(formData);
            var form = reader.ReadForm().ToDictionary(k => k.Key, v => v.Value.ToString(), StringComparer.OrdinalIgnoreCase);
            return new StringToStringDictionaryBackplaneStrategy(form);
        }

        public bool TryGetValue(string key, out ReadOnlySpan<char> value)
        {
            var propertyName = key.Replace("get_", "", StringComparison.OrdinalIgnoreCase);
            if (form.TryGetValue(propertyName, out var valueString))
            {
                value = valueString.AsSpan();
                return true;
            }

            value = default;
            return false;
        }
    }

    public class MemoryOfCharToStringDictionaryBackplaneStrategy(IReadOnlyDictionary<ReadOnlyMemory<char>, string> form) : IBackplaneStrategy
    {
        public static IBackplaneStrategy Create(string formData)
        {
            using var reader = new FormReader(formData);
            var form = reader.ReadForm().ToDictionary(k => k.Key.AsMemory(), v => v.Value.ToString(), new MemStringEqualityComparer());
            return new MemoryOfCharToStringDictionaryBackplaneStrategy(form);
        }

        public bool TryGetValue(string key, out ReadOnlySpan<char> value)
        {
            var propertyName = key.AsMemory().Slice(key.IndexOf('_') + 1);
            if (form.TryGetValue(propertyName, out var valueString))
            {
                value = valueString.AsSpan();
                return true;
            }

            value = default;
            return false;
        }
    }

    public class MemoryOfCharToMemoryOfCharDictionaryBackplaneStrategy(IReadOnlyDictionary<ReadOnlyMemory<char>, ReadOnlyMemory<char>> form) : IBackplaneStrategy
    {
        public static IBackplaneStrategy Create(string formData)
        {
            var form = new Dictionary<ReadOnlyMemory<char>, ReadOnlyMemory<char>>(new MemStringEqualityComparer());
            foreach (var memberRange in formData.AsSpan().Split('&'))
            {
                var member = formData.AsMemory()[memberRange];
                var memberNameValueEnumerator = member.Span.Split('=');
                memberNameValueEnumerator.MoveNext();
                var key = member[memberNameValueEnumerator.Current];
                memberNameValueEnumerator.MoveNext();
                var value = member[memberNameValueEnumerator.Current];
                form.Add(key, value);
            }

            return new MemoryOfCharToMemoryOfCharDictionaryBackplaneStrategy(form);
        }
        public bool TryGetValue(string key, out ReadOnlySpan<char> value)
        {
            var propertyName = key.AsMemory().Slice(key.IndexOf('_') + 1);
            if (form.TryGetValue(propertyName, out var valueString))
            {
                value = valueString.Span;
                return true;
            }
            value = default;
            return false;
        }
    }

    public class FormDataParser : DispatchProxy
    {
        // 1:
        private IReadOnlyDictionary<string, string> formData = null!;
        // 2: private IReadOnlyDictionary<ReadOnlyMemory<char>, string> formData = null!;
        // 3: private IReadOnlyDictionary<ReadOnlyMemory<char>, ReadOnlyMemory<char>> formData = null!;

        public static T Parse<T>(string formData)
        {
            Debug.Assert(typeof(T).IsInterface, "T must be an interface type");

            using var b = new FormReader(formData);

            // 3:
            //var d = new Dictionary<ReadOnlyMemory<char>, ReadOnlyMemory<char>>(new MemStringEqualityComparer());
            //foreach (var a in formData.AsSpan().Split('&'))
            //{
            //    var m = formData.AsMemory()[a];
            //    var c = m.Span.Split('=');
            //    c.MoveNext();
            //    var key = m[c.Current];
            //    c.MoveNext();
            //    var value = m[c.Current];
            //    d.Add(key, value);
            //}


            var parser = Create<T, FormDataParser>();
            // 1:
            (parser as FormDataParser)!.formData = b.ReadForm().ToDictionary(k => k.Key, v => v.Value.ToString(), StringComparer.OrdinalIgnoreCase);
            // 2: (parser as FormDataParser)!.formData = b.ReadForm().ToDictionary(k => k.Key.AsMemory(), v => v.Value.ToString(), new MemStringEqualityComparer());
            // 3: (parser as FormDataParser)!.formData = d;
            return (T)parser;
        }

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            // 1:
            var propName = targetMethod.Name.Replace("get_", "");
            // 2, 3
            //var propName = targetMethod.Name.IndexOf('_') switch
            //{
            //    -1 => targetMethod.Name.AsMemory(),
            //    int i => targetMethod.Name.AsMemory().Slice(targetMethod.Name.IndexOf('_') + 1)
            //};

            if (formData.TryGetValue(propName, out var value))
            {
                // 1, 2:
                return Convert.ChangeType(value, targetMethod.ReturnType, CultureInfo.InvariantCulture);
                // 3:
                //if (targetMethod.ReturnType == typeof(string))
                //{
                //    return HttpUtility.UrlDecode(new string(value.Span));
                //}
                //if (targetMethod.ReturnType == typeof(int))
                //{
                //    return int.Parse(value.Span, CultureInfo.InvariantCulture);
                //}
            }

            return null;
        }
    }

    // 2:
    public class MemStringEqualityComparer : IEqualityComparer<ReadOnlyMemory<char>>
    {
        public int GetHashCode(ReadOnlyMemory<char> obj) =>
            string.GetHashCode(obj.Span, StringComparison.OrdinalIgnoreCase);

        public bool Equals(ReadOnlyMemory<char> x, ReadOnlyMemory<char> y) =>
            MemoryExtensions.Equals(x.Span, y.Span, StringComparison.OrdinalIgnoreCase);
    }
}
