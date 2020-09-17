using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private static IObscureDuckType[] proxies;

        static ValueTypePropertyBenchmark()
        {
            proxies = new IObscureDuckType[]
            {
                ObscureObject.GetPropertyPublicObject().As<IObscureDuckType>(),
                ObscureObject.GetPropertyInternalObject().As<IObscureDuckType>(),
                ObscureObject.GetPropertyPrivateObject().As<IObscureDuckType>()
            };
        }

        [ParamsAllValues]
        public InstanceTypes InstanceType { get; set; }

        public IObscureDuckType Proxy => proxies[(int)InstanceType];

        public enum InstanceTypes
        {
            Public, 
            Internal,
            Private
        }

        /**
         * Get Static Property
         */

        [Benchmark]
        public int GetPublicStaticProperty()
        {
            return Proxy.PublicStaticGetSetValueType;
        }

        [Benchmark]
        public int GetInternalStaticProperty()
        {
            return Proxy.InternalStaticGetSetValueType;
        }

        [Benchmark]
        public int GetProtectedStaticProperty()
        {
            return Proxy.ProtectedStaticGetSetValueType;
        }

        [Benchmark]
        public int GetPrivateStaticProperty()
        {
            return Proxy.PrivateStaticGetSetValueType;
        }


        /**
         * Set Static Property
         */

        [Benchmark]
        public void SetPublicStaticProperty()
        {
            Proxy.PublicStaticGetSetValueType = 42;
        }

        [Benchmark]
        public void SetInternalStaticProperty()
        {
            Proxy.InternalStaticGetSetValueType = 42;
        }

        [Benchmark]
        public void SetProtectedStaticProperty()
        {
            Proxy.ProtectedStaticGetSetValueType = 42;
        }

        [Benchmark]
        public void SetPrivateStaticProperty()
        {
            Proxy.PrivateStaticGetSetValueType = 42;
        }


        /**
         * Get Property
         */

        [Benchmark]
        public int GetPublicProperty()
        {
            return Proxy.PublicGetSetValueType;
        }

        [Benchmark]
        public int GetInternalProperty()
        {
            return Proxy.InternalGetSetValueType;
        }

        [Benchmark]
        public int GetProtectedProperty()
        {
            return Proxy.ProtectedGetSetValueType;
        }

        [Benchmark]
        public int GetPrivateProperty()
        {
            return Proxy.PrivateGetSetValueType;
        }


        /**
         * Set Property
         */

        [Benchmark]
        public void SetPublicProperty()
        {
            Proxy.PublicGetSetValueType = 42;
        }

        [Benchmark]
        public void SetInternalProperty()
        {
            Proxy.InternalGetSetValueType = 42;
        }

        [Benchmark]
        public void SetProtectedProperty()
        {
            Proxy.ProtectedGetSetValueType = 42;
        }

        [Benchmark]
        public void SetPrivateProperty()
        {
            Proxy.PrivateGetSetValueType = 42;
        }


        /**
         * Indexer
         */

        [Benchmark]
        public int GetIndexerProperty()
        {
            return Proxy[42];
        }

        [Benchmark]
        public void SetIndexerProperty()
        {
            Proxy[42] = 42;
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
