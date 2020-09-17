using System.Collections.Generic;

namespace Datadog.Trace
{
    internal interface ITags
    {
        string GetTag(string key);

        void SetTag(string key, string value);

        double? GetMetric(string key);

        void SetMetric(string key, double? value);

        IEnumerable<TagsDictionary.Key> GetKeys();

        int SerializeTo(ref byte[] buffer, int offset);
    }
}
