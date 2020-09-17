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
    public class ValueTypeFieldBenchmark
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
            yield return new ProxyItem("Public", ObscureObject.GetFieldPublicObject().As<IObscureDuckType>());
            yield return new ProxyItem("Internal", ObscureObject.GetFieldInternalObject().As<IObscureDuckType>());
            yield return new ProxyItem("Private", ObscureObject.GetFieldPrivateObject().As<IObscureDuckType>());
        }

        [Benchmark(Baseline = true)]
        public IObscureDuckType GetProxy()
        {
            return InstanceType.Proxy;
        }

        /**
         * Get Static Field
         */

        [Benchmark]
        public int GetPublicStaticField()
        {
            return InstanceType.Proxy.PublicStaticValueTypeField;
        }

        [Benchmark]
        public int GetInternalStaticField()
        {
            return InstanceType.Proxy.InternalStaticValueTypeField;
        }

        [Benchmark]
        public int GetProtectedStaticField()
        {
            return InstanceType.Proxy.ProtectedStaticValueTypeField;
        }

        [Benchmark]
        public int GetPrivateStaticField()
        {
            return InstanceType.Proxy.PrivateStaticValueTypeField;
        }


        /**
         * Set Static Field
         */

        [Benchmark]
        public void SetPublicStaticField()
        {
            InstanceType.Proxy.PublicStaticValueTypeField = 42;
        }

        [Benchmark]
        public void SetInternalStaticField()
        {
            InstanceType.Proxy.InternalStaticValueTypeField = 42;
        }

        [Benchmark]
        public void SetProtectedStaticField()
        {
            InstanceType.Proxy.ProtectedStaticValueTypeField = 42;
        }

        [Benchmark]
        public void SetPrivateStaticField()
        {
            InstanceType.Proxy.PrivateStaticValueTypeField = 42;
        }


        /**
         * Get Field
         */

        [Benchmark]
        public int GetPublicField()
        {
            return InstanceType.Proxy.PublicValueTypeField;
        }

        [Benchmark]
        public int GetInternalField()
        {
            return InstanceType.Proxy.InternalValueTypeField;
        }

        [Benchmark]
        public int GetProtectedField()
        {
            return InstanceType.Proxy.ProtectedValueTypeField;
        }

        [Benchmark]
        public int GetPrivateField()
        {
            return InstanceType.Proxy.PrivateValueTypeField;
        }


        /**
         * Set Field
         */

        [Benchmark]
        public void SetPublicField()
        {
            InstanceType.Proxy.PublicValueTypeField = 42;
        }

        [Benchmark]
        public void SetInternalField()
        {
            InstanceType.Proxy.InternalValueTypeField = 42;
        }

        [Benchmark]
        public void SetProtectedField()
        {
            InstanceType.Proxy.ProtectedValueTypeField = 42;
        }

        [Benchmark]
        public void SetPrivateField()
        {
            InstanceType.Proxy.PrivateValueTypeField = 42;
        }


        public interface IObscureDuckType
        {
            [Duck(Name = "_publicStaticValueTypeField", Kind = DuckKind.Field)]
            int PublicStaticValueTypeField { get; set; }

            [Duck(Name = "_internalStaticValueTypeField", Kind = DuckKind.Field)]
            int InternalStaticValueTypeField { get; set; }

            [Duck(Name = "_protectedStaticValueTypeField", Kind = DuckKind.Field)]
            int ProtectedStaticValueTypeField { get; set; }

            [Duck(Name = "_privateStaticValueTypeField", Kind = DuckKind.Field)]
            int PrivateStaticValueTypeField { get; set; }

            // *

            [Duck(Name = "_publicValueTypeField", Kind = DuckKind.Field)]
            int PublicValueTypeField { get; set; }

            [Duck(Name = "_internalValueTypeField", Kind = DuckKind.Field)]
            int InternalValueTypeField { get; set; }

            [Duck(Name = "_protectedValueTypeField", Kind = DuckKind.Field)]
            int ProtectedValueTypeField { get; set; }

            [Duck(Name = "_privateValueTypeField", Kind = DuckKind.Field)]
            int PrivateValueTypeField { get; set; }
        }
    }
}
