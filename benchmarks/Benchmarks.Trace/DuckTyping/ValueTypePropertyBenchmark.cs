using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using Datadog.Trace.BenchmarkDotNet;
using Datadog.Trace.ClrProfiler.DuckTyping;

#pragma warning disable 414

namespace Benchmarks.Trace.DuckTyping
{
    [DatadogExporter]
    [MemoryDiagnoser]
    public class ValueTypePropertyBenchmark
    {
        [ParamsSource(nameof(Proxies))]
        public ProxyItem InstanceType { get; set; }

        public readonly struct ProxyItem
        {
            private readonly string _name;
            public readonly IObscureDuckType Proxy;

            public ProxyItem(string name, IObscureDuckType proxy)
            {
                _name = name;
                Proxy = proxy;
            }

            public override string ToString()
            {
                return _name;
            }
        }

        public static IEnumerable<ProxyItem> Proxies()
        {
            yield return new ProxyItem("Public", ObscureObject.GetPropertyPublicObject().As<IObscureDuckType>());
            yield return new ProxyItem("Internal", ObscureObject.GetPropertyInternalObject().As<IObscureDuckType>());
            yield return new ProxyItem("Private", ObscureObject.GetPropertyPrivateObject().As<IObscureDuckType>());
        }

        [Benchmark(Baseline = true)]
        public IObscureDuckType GetProxy()
        {
            return InstanceType.Proxy;
        }

        /**
         * Get Static Property
         */

        [Benchmark]
        public int GetPublicStaticProperty()
        {
            return InstanceType.Proxy.PublicStaticGetSetValueType;
        }

        [Benchmark]
        public int GetInternalStaticProperty()
        {
            return InstanceType.Proxy.InternalStaticGetSetValueType;
        }

        [Benchmark]
        public int GetProtectedStaticProperty()
        {
            return InstanceType.Proxy.ProtectedStaticGetSetValueType;
        }

        [Benchmark]
        public int GetPrivateStaticProperty()
        {
            return InstanceType.Proxy.PrivateStaticGetSetValueType;
        }


        /**
         * Set Static Property
         */

        [Benchmark]
        public void SetPublicStaticProperty()
        {
            InstanceType.Proxy.PublicStaticGetSetValueType = 42;
        }

        [Benchmark]
        public void SetInternalStaticProperty()
        {
            InstanceType.Proxy.InternalStaticGetSetValueType = 42;
        }

        [Benchmark]
        public void SetProtectedStaticProperty()
        {
            InstanceType.Proxy.ProtectedStaticGetSetValueType = 42;
        }

        [Benchmark]
        public void SetPrivateStaticProperty()
        {
            InstanceType.Proxy.PrivateStaticGetSetValueType = 42;
        }


        /**
         * Get Property
         */

        [Benchmark]
        public int GetPublicProperty()
        {
            return InstanceType.Proxy.PublicGetSetValueType;
        }

        [Benchmark]
        public int GetInternalProperty()
        {
            return InstanceType.Proxy.InternalGetSetValueType;
        }

        [Benchmark]
        public int GetProtectedProperty()
        {
            return InstanceType.Proxy.ProtectedGetSetValueType;
        }

        [Benchmark]
        public int GetPrivateProperty()
        {
            return InstanceType.Proxy.PrivateGetSetValueType;
        }


        /**
         * Set Property
         */

        [Benchmark]
        public void SetPublicProperty()
        {
            InstanceType.Proxy.PublicGetSetValueType = 42;
        }

        [Benchmark]
        public void SetInternalProperty()
        {
            InstanceType.Proxy.InternalGetSetValueType = 42;
        }

        [Benchmark]
        public void SetProtectedProperty()
        {
            InstanceType.Proxy.ProtectedGetSetValueType = 42;
        }

        [Benchmark]
        public void SetPrivateProperty()
        {
            InstanceType.Proxy.PrivateGetSetValueType = 42;
        }


        /**
         * Indexer
         */

        [Benchmark]
        public int GetIndexerProperty()
        {
            return InstanceType.Proxy[42];
        }

        [Benchmark]
        public void SetIndexerProperty()
        {
            InstanceType.Proxy[42] = 42;
        }

        public interface IObscureDuckType
        {
            int PublicStaticGetSetValueType { get; set; }

            int InternalStaticGetSetValueType { get; set; }

            int ProtectedStaticGetSetValueType { get; set; }

            int PrivateStaticGetSetValueType { get; set; }

            // *

            int PublicGetSetValueType { get; set; }

            int InternalGetSetValueType { get; set; }

            int ProtectedGetSetValueType { get; set; }

            int PrivateGetSetValueType { get; set; }

            // *

            int this[int index] { get; set; }
        }
    }
}
