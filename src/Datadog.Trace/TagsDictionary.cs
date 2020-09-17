using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Datadog.Trace
{
    // TODO: Locking
    internal class TagsDictionary : Dictionary<TagsDictionary.Key, TagsDictionary.Value>, ITags
    {
        public string GetTag(string key)
        {
            return TryGetValue(new Key(key, isMetric: false), out var value) ? value.StringValue : null;
        }

        public void SetTag(string key, string value)
        {
            var dictionaryKey = new Key(key, isMetric: false);

            if (value == null)
            {
                Remove(dictionaryKey);
                return;
            }

            this[dictionaryKey] = value;
        }

        public double? GetMetric(string key)
        {
            return TryGetValue(new Key(key, isMetric: true), out var value) ? value.DoubleValue : (double?)null;
        }

        public void SetMetric(string key, double? value)
        {
            var dictionaryKey = new Key(key, isMetric: true);

            if (value == null)
            {
                Remove(dictionaryKey);
                return;
            }

            this[dictionaryKey] = value.Value;
        }

        public IEnumerable<Key> GetKeys() => Keys;

        public int SerializeTo(ref byte[] buffer, int offset)
        {
            throw new NotImplementedException();
        }

        internal readonly struct Key : IEquatable<Key>
        {
            public readonly string Label;
            public readonly bool IsMetric;

            public Key(string key, bool isMetric)
            {
                Label = key;
                IsMetric = isMetric;
            }

            public bool Equals(Key other)
            {
                return Label == other.Label && IsMetric == other.IsMetric;
            }

            public override bool Equals(object obj)
            {
                return obj is Key other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (Label.GetHashCode() * 397) ^ IsMetric.GetHashCode();
                }
            }

            public override string ToString()
            {
                return $"{Label} ({(IsMetric ? "metric" : "tag")})";
            }
        }

        internal readonly struct Value
        {
            public readonly string StringValue;

            public readonly double DoubleValue;

            public Value(string value)
            {
                DoubleValue = 0;
                StringValue = value;
            }

            public Value(double value)
            {
                StringValue = null;
                DoubleValue = value;
            }

            public static implicit operator Value(string val) => new Value(val);

            public static implicit operator Value(double val) => new Value(val);
        }
    }
}
