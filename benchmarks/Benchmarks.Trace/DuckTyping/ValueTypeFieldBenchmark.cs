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
        private static IObscureDuckType[] proxies;

        static ValueTypeFieldBenchmark()
        {
            proxies = new IObscureDuckType[]
            {
                ObscureObject.GetFieldPublicObject().As<IObscureDuckType>(),
                ObscureObject.GetFieldInternalObject().As<IObscureDuckType>(),
                ObscureObject.GetFieldPrivateObject().As<IObscureDuckType>()
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
         * Get Static Field
         */

        [Benchmark]
        public int GetPublicStaticField()
        {
            return Proxy.PublicStaticValueTypeField;
        }

        [Benchmark]
        public int GetInternalStaticField()
        {
            return Proxy.InternalStaticValueTypeField;
        }

        [Benchmark]
        public int GetProtectedStaticField()
        {
            return Proxy.ProtectedStaticValueTypeField;
        }

        [Benchmark]
        public int GetPrivateStaticField()
        {
            return Proxy.PrivateStaticValueTypeField;
        }


        /**
         * Set Static Field
         */

        [Benchmark]
        public void SetPublicStaticField()
        {
            Proxy.PublicStaticValueTypeField = 42;
        }

        [Benchmark]
        public void SetInternalStaticField()
        {
            Proxy.InternalStaticValueTypeField = 42;
        }

        [Benchmark]
        public void SetProtectedStaticField()
        {
            Proxy.ProtectedStaticValueTypeField = 42;
        }

        [Benchmark]
        public void SetPrivateStaticField()
        {
            Proxy.PrivateStaticValueTypeField = 42;
        }


        /**
         * Get Field
         */

        [Benchmark]
        public int GetPublicField()
        {
            return Proxy.PublicValueTypeField;
        }

        [Benchmark]
        public int GetInternalField()
        {
            return Proxy.InternalValueTypeField;
        }

        [Benchmark]
        public int GetProtectedField()
        {
            return Proxy.ProtectedValueTypeField;
        }

        [Benchmark]
        public int GetPrivateField()
        {
            return Proxy.PrivateValueTypeField;
        }


        /**
         * Set Field
         */

        [Benchmark]
        public void SetPublicField()
        {
            Proxy.PublicValueTypeField = 42;
        }

        [Benchmark]
        public void SetInternalField()
        {
            Proxy.InternalValueTypeField = 42;
        }

        [Benchmark]
        public void SetProtectedField()
        {
            Proxy.ProtectedValueTypeField = 42;
        }

        [Benchmark]
        public void SetPrivateField()
        {
            Proxy.PrivateValueTypeField = 42;
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
