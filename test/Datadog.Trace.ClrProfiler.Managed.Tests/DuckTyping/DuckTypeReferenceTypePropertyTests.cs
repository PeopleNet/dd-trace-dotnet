using System.Collections.Generic;
using Datadog.Trace.ClrProfiler.DuckTyping;
using Xunit;

#pragma warning disable SA1201 // Elements must appear in the correct order

namespace Datadog.Trace.ClrProfiler.Managed.Tests.DuckTyping
{
    public class DuckTypeReferenceTypePropertyTests
    {
        public static IEnumerable<object[]> Data()
        {
            return new[]
            {
                new object[] { ObscureObject.GetPropertyPublicObject() },
                new object[] { ObscureObject.GetPropertyInternalObject() },
                new object[] { ObscureObject.GetPropertyPrivateObject() },
            };
        }

        [Theory]
        [MemberData(nameof(Data))]
        public void StaticPropertyOnlySetException(object obscureObject)
        {
            Assert.Throws<DuckTypePropertyCantBeWrittenException>(() =>
            {
                obscureObject.As<IObscureStaticErrorDuckType>();
            });
        }

        [Theory]
        [MemberData(nameof(Data))]
        public void PropertyOnlySetException(object obscureObject)
        {
            Assert.Throws<DuckTypePropertyCantBeWrittenException>(() =>
            {
                obscureObject.As<IObscureErrorDuckType>();
            });
        }

        [Theory]
        [MemberData(nameof(Data))]
        public void StaticOnlyGetProperties(object obscureObject)
        {
            var duckInterface = obscureObject.As<IObscureDuckType>();
            var duckAbstract = obscureObject.As<ObscureDuckTypeAbstractClass>();
            var duckVirtual = obscureObject.As<ObscureDuckType>();

            // *
            Assert.Equal("10", duckInterface.PublicStaticGetReferenceType);
            Assert.Equal("10", duckAbstract.PublicStaticGetReferenceType);
            Assert.Equal("10", duckVirtual.PublicStaticGetReferenceType);

            // *
            Assert.Equal("11", duckInterface.InternalStaticGetReferenceType);
            Assert.Equal("11", duckAbstract.InternalStaticGetReferenceType);
            Assert.Equal("11", duckVirtual.InternalStaticGetReferenceType);

            // *
            Assert.Equal("12", duckInterface.ProtectedStaticGetReferenceType);
            Assert.Equal("12", duckAbstract.ProtectedStaticGetReferenceType);
            Assert.Equal("12", duckVirtual.ProtectedStaticGetReferenceType);

            // *
            Assert.Equal("13", duckInterface.PrivateStaticGetReferenceType);
            Assert.Equal("13", duckAbstract.PrivateStaticGetReferenceType);
            Assert.Equal("13", duckVirtual.PrivateStaticGetReferenceType);
        }

        [Theory]
        [MemberData(nameof(Data))]
        public void StaticProperties(object obscureObject)
        {
            var duckInterface = obscureObject.As<IObscureDuckType>();
            var duckAbstract = obscureObject.As<ObscureDuckTypeAbstractClass>();
            var duckVirtual = obscureObject.As<ObscureDuckType>();

            Assert.Equal("20", duckInterface.PublicStaticGetSetReferenceType);
            Assert.Equal("20", duckAbstract.PublicStaticGetSetReferenceType);
            Assert.Equal("20", duckVirtual.PublicStaticGetSetReferenceType);

            duckInterface.PublicStaticGetSetReferenceType = "42";
            Assert.Equal("42", duckInterface.PublicStaticGetSetReferenceType);
            Assert.Equal("42", duckAbstract.PublicStaticGetSetReferenceType);
            Assert.Equal("42", duckVirtual.PublicStaticGetSetReferenceType);

            duckAbstract.PublicStaticGetSetReferenceType = "50";
            Assert.Equal("50", duckInterface.PublicStaticGetSetReferenceType);
            Assert.Equal("50", duckAbstract.PublicStaticGetSetReferenceType);
            Assert.Equal("50", duckVirtual.PublicStaticGetSetReferenceType);

            duckVirtual.PublicStaticGetSetReferenceType = "60";
            Assert.Equal("60", duckInterface.PublicStaticGetSetReferenceType);
            Assert.Equal("60", duckAbstract.PublicStaticGetSetReferenceType);
            Assert.Equal("60", duckVirtual.PublicStaticGetSetReferenceType);

            // *

            Assert.Equal("21", duckInterface.InternalStaticGetSetReferenceType);
            Assert.Equal("21", duckAbstract.InternalStaticGetSetReferenceType);
            Assert.Equal("21", duckVirtual.InternalStaticGetSetReferenceType);

            duckInterface.InternalStaticGetSetReferenceType = "42";
            Assert.Equal("42", duckInterface.InternalStaticGetSetReferenceType);
            Assert.Equal("42", duckAbstract.InternalStaticGetSetReferenceType);
            Assert.Equal("42", duckVirtual.InternalStaticGetSetReferenceType);

            duckAbstract.InternalStaticGetSetReferenceType = "50";
            Assert.Equal("50", duckInterface.InternalStaticGetSetReferenceType);
            Assert.Equal("50", duckAbstract.InternalStaticGetSetReferenceType);
            Assert.Equal("50", duckVirtual.InternalStaticGetSetReferenceType);

            duckVirtual.InternalStaticGetSetReferenceType = "60";
            Assert.Equal("60", duckInterface.InternalStaticGetSetReferenceType);
            Assert.Equal("60", duckAbstract.InternalStaticGetSetReferenceType);
            Assert.Equal("60", duckVirtual.InternalStaticGetSetReferenceType);

            // *

            Assert.Equal("22", duckInterface.ProtectedStaticGetSetReferenceType);
            Assert.Equal("22", duckAbstract.ProtectedStaticGetSetReferenceType);
            Assert.Equal("22", duckVirtual.ProtectedStaticGetSetReferenceType);

            duckInterface.ProtectedStaticGetSetReferenceType = "42";
            Assert.Equal("42", duckInterface.ProtectedStaticGetSetReferenceType);
            Assert.Equal("42", duckAbstract.ProtectedStaticGetSetReferenceType);
            Assert.Equal("42", duckVirtual.ProtectedStaticGetSetReferenceType);

            duckAbstract.ProtectedStaticGetSetReferenceType = "50";
            Assert.Equal("50", duckInterface.ProtectedStaticGetSetReferenceType);
            Assert.Equal("50", duckAbstract.ProtectedStaticGetSetReferenceType);
            Assert.Equal("50", duckVirtual.ProtectedStaticGetSetReferenceType);

            duckVirtual.ProtectedStaticGetSetReferenceType = "60";
            Assert.Equal("60", duckInterface.ProtectedStaticGetSetReferenceType);
            Assert.Equal("60", duckAbstract.ProtectedStaticGetSetReferenceType);
            Assert.Equal("60", duckVirtual.ProtectedStaticGetSetReferenceType);

            // *

            Assert.Equal("23", duckInterface.PrivateStaticGetSetReferenceType);
            Assert.Equal("23", duckAbstract.PrivateStaticGetSetReferenceType);
            Assert.Equal("23", duckVirtual.PrivateStaticGetSetReferenceType);

            duckInterface.PrivateStaticGetSetReferenceType = "42";
            Assert.Equal("42", duckInterface.PrivateStaticGetSetReferenceType);
            Assert.Equal("42", duckAbstract.PrivateStaticGetSetReferenceType);
            Assert.Equal("42", duckVirtual.PrivateStaticGetSetReferenceType);

            duckAbstract.PrivateStaticGetSetReferenceType = "50";
            Assert.Equal("50", duckInterface.PrivateStaticGetSetReferenceType);
            Assert.Equal("50", duckAbstract.PrivateStaticGetSetReferenceType);
            Assert.Equal("50", duckVirtual.PrivateStaticGetSetReferenceType);

            duckVirtual.PrivateStaticGetSetReferenceType = "60";
            Assert.Equal("60", duckInterface.PrivateStaticGetSetReferenceType);
            Assert.Equal("60", duckAbstract.PrivateStaticGetSetReferenceType);
            Assert.Equal("60", duckVirtual.PrivateStaticGetSetReferenceType);
        }

        [Theory]
        [MemberData(nameof(Data))]
        public void OnlyGetProperties(object obscureObject)
        {
            var duckInterface = obscureObject.As<IObscureDuckType>();
            var duckAbstract = obscureObject.As<ObscureDuckTypeAbstractClass>();
            var duckVirtual = obscureObject.As<ObscureDuckType>();

            // *
            Assert.Equal("30", duckInterface.PublicGetReferenceType);
            Assert.Equal("30", duckAbstract.PublicGetReferenceType);
            Assert.Equal("30", duckVirtual.PublicGetReferenceType);

            // *
            Assert.Equal("31", duckInterface.InternalGetReferenceType);
            Assert.Equal("31", duckAbstract.InternalGetReferenceType);
            Assert.Equal("31", duckVirtual.InternalGetReferenceType);

            // *
            Assert.Equal("32", duckInterface.ProtectedGetReferenceType);
            Assert.Equal("32", duckAbstract.ProtectedGetReferenceType);
            Assert.Equal("32", duckVirtual.ProtectedGetReferenceType);

            // *
            Assert.Equal("33", duckInterface.PrivateGetReferenceType);
            Assert.Equal("33", duckAbstract.PrivateGetReferenceType);
            Assert.Equal("33", duckVirtual.PrivateGetReferenceType);
        }

        [Theory]
        [MemberData(nameof(Data))]
        public void Properties(object obscureObject)
        {
            var duckInterface = obscureObject.As<IObscureDuckType>();
            var duckAbstract = obscureObject.As<ObscureDuckTypeAbstractClass>();
            var duckVirtual = obscureObject.As<ObscureDuckType>();

            Assert.Equal("40", duckInterface.PublicGetSetReferenceType);
            Assert.Equal("40", duckAbstract.PublicGetSetReferenceType);
            Assert.Equal("40", duckVirtual.PublicGetSetReferenceType);

            duckInterface.PublicGetSetReferenceType = "42";
            Assert.Equal("42", duckInterface.PublicGetSetReferenceType);
            Assert.Equal("42", duckAbstract.PublicGetSetReferenceType);
            Assert.Equal("42", duckVirtual.PublicGetSetReferenceType);

            duckAbstract.PublicGetSetReferenceType = "50";
            Assert.Equal("50", duckInterface.PublicGetSetReferenceType);
            Assert.Equal("50", duckAbstract.PublicGetSetReferenceType);
            Assert.Equal("50", duckVirtual.PublicGetSetReferenceType);

            duckVirtual.PublicGetSetReferenceType = "60";
            Assert.Equal("60", duckInterface.PublicGetSetReferenceType);
            Assert.Equal("60", duckAbstract.PublicGetSetReferenceType);
            Assert.Equal("60", duckVirtual.PublicGetSetReferenceType);

            // *

            Assert.Equal("41", duckInterface.InternalGetSetReferenceType);
            Assert.Equal("41", duckAbstract.InternalGetSetReferenceType);
            Assert.Equal("41", duckVirtual.InternalGetSetReferenceType);

            duckInterface.InternalGetSetReferenceType = "42";
            Assert.Equal("42", duckInterface.InternalGetSetReferenceType);
            Assert.Equal("42", duckAbstract.InternalGetSetReferenceType);
            Assert.Equal("42", duckVirtual.InternalGetSetReferenceType);

            duckAbstract.InternalGetSetReferenceType = "50";
            Assert.Equal("50", duckInterface.InternalGetSetReferenceType);
            Assert.Equal("50", duckAbstract.InternalGetSetReferenceType);
            Assert.Equal("50", duckVirtual.InternalGetSetReferenceType);

            duckVirtual.InternalGetSetReferenceType = "60";
            Assert.Equal("60", duckInterface.InternalGetSetReferenceType);
            Assert.Equal("60", duckAbstract.InternalGetSetReferenceType);
            Assert.Equal("60", duckVirtual.InternalGetSetReferenceType);

            // *

            Assert.Equal("42", duckInterface.ProtectedGetSetReferenceType);
            Assert.Equal("42", duckAbstract.ProtectedGetSetReferenceType);
            Assert.Equal("42", duckVirtual.ProtectedGetSetReferenceType);

            duckInterface.ProtectedGetSetReferenceType = "45";
            Assert.Equal("45", duckInterface.ProtectedGetSetReferenceType);
            Assert.Equal("45", duckAbstract.ProtectedGetSetReferenceType);
            Assert.Equal("45", duckVirtual.ProtectedGetSetReferenceType);

            duckAbstract.ProtectedGetSetReferenceType = "50";
            Assert.Equal("50", duckInterface.ProtectedGetSetReferenceType);
            Assert.Equal("50", duckAbstract.ProtectedGetSetReferenceType);
            Assert.Equal("50", duckVirtual.ProtectedGetSetReferenceType);

            duckVirtual.ProtectedGetSetReferenceType = "60";
            Assert.Equal("60", duckInterface.ProtectedGetSetReferenceType);
            Assert.Equal("60", duckAbstract.ProtectedGetSetReferenceType);
            Assert.Equal("60", duckVirtual.ProtectedGetSetReferenceType);

            // *

            Assert.Equal("43", duckInterface.PrivateGetSetReferenceType);
            Assert.Equal("43", duckAbstract.PrivateGetSetReferenceType);
            Assert.Equal("43", duckVirtual.PrivateGetSetReferenceType);

            duckInterface.PrivateGetSetReferenceType = "42";
            Assert.Equal("42", duckInterface.PrivateGetSetReferenceType);
            Assert.Equal("42", duckAbstract.PrivateGetSetReferenceType);
            Assert.Equal("42", duckVirtual.PrivateGetSetReferenceType);

            duckAbstract.PrivateGetSetReferenceType = "50";
            Assert.Equal("50", duckInterface.PrivateGetSetReferenceType);
            Assert.Equal("50", duckAbstract.PrivateGetSetReferenceType);
            Assert.Equal("50", duckVirtual.PrivateGetSetReferenceType);

            duckVirtual.PrivateGetSetReferenceType = "60";
            Assert.Equal("60", duckInterface.PrivateGetSetReferenceType);
            Assert.Equal("60", duckAbstract.PrivateGetSetReferenceType);
            Assert.Equal("60", duckVirtual.PrivateGetSetReferenceType);
        }

        [Theory]
        [MemberData(nameof(Data))]
        public void Indexer(object obscureObject)
        {
            var duckInterface = obscureObject.As<IObscureDuckType>();
            var duckAbstract = obscureObject.As<ObscureDuckTypeAbstractClass>();
            var duckVirtual = obscureObject.As<ObscureDuckType>();

            duckInterface["1"] = "100";
            Assert.Equal("100", duckInterface["1"]);
            Assert.Equal("100", duckAbstract["1"]);
            Assert.Equal("100", duckVirtual["1"]);

            duckAbstract["2"] = "200";
            Assert.Equal("200", duckInterface["2"]);
            Assert.Equal("200", duckAbstract["2"]);
            Assert.Equal("200", duckVirtual["2"]);

            duckVirtual["3"] = "300";
            Assert.Equal("300", duckInterface["3"]);
            Assert.Equal("300", duckAbstract["3"]);
            Assert.Equal("300", duckVirtual["3"]);
        }

        public interface IObscureDuckType
        {
            string PublicStaticGetReferenceType { get; }

            string InternalStaticGetReferenceType { get; }

            string ProtectedStaticGetReferenceType { get; }

            string PrivateStaticGetReferenceType { get; }

            // *

            string PublicStaticGetSetReferenceType { get; set; }

            string InternalStaticGetSetReferenceType { get; set; }

            string ProtectedStaticGetSetReferenceType { get; set; }

            string PrivateStaticGetSetReferenceType { get; set; }

            // *

            string PublicGetReferenceType { get; }

            string InternalGetReferenceType { get; }

            string ProtectedGetReferenceType { get; }

            string PrivateGetReferenceType { get; }

            // *

            string PublicGetSetReferenceType { get; set; }

            string InternalGetSetReferenceType { get; set; }

            string ProtectedGetSetReferenceType { get; set; }

            string PrivateGetSetReferenceType { get; set; }

            // *

            string this[string index] { get; set; }
        }

        public interface IObscureStaticErrorDuckType
        {
            string PublicStaticGetReferenceType { get; set; }
        }

        public interface IObscureErrorDuckType
        {
            string PublicGetReferenceType { get; set; }
        }

        public abstract class ObscureDuckTypeAbstractClass
        {
            public abstract string PublicStaticGetReferenceType { get; }

            public abstract string InternalStaticGetReferenceType { get; }

            public abstract string ProtectedStaticGetReferenceType { get; }

            public abstract string PrivateStaticGetReferenceType { get; }

            // *

            public abstract string PublicStaticGetSetReferenceType { get; set; }

            public abstract string InternalStaticGetSetReferenceType { get; set; }

            public abstract string ProtectedStaticGetSetReferenceType { get; set; }

            public abstract string PrivateStaticGetSetReferenceType { get; set; }

            // *

            public abstract string PublicGetReferenceType { get; }

            public abstract string InternalGetReferenceType { get; }

            public abstract string ProtectedGetReferenceType { get; }

            public abstract string PrivateGetReferenceType { get; }

            // *

            public abstract string PublicGetSetReferenceType { get; set; }

            public abstract string InternalGetSetReferenceType { get; set; }

            public abstract string ProtectedGetSetReferenceType { get; set; }

            public abstract string PrivateGetSetReferenceType { get; set; }

            // *

            public abstract string this[string index] { get; set; }
        }

        public class ObscureDuckType
        {
            public virtual string PublicStaticGetReferenceType { get; }

            public virtual string InternalStaticGetReferenceType { get; }

            public virtual string ProtectedStaticGetReferenceType { get; }

            public virtual string PrivateStaticGetReferenceType { get; }

            // *

            public virtual string PublicStaticGetSetReferenceType { get; set; }

            public virtual string InternalStaticGetSetReferenceType { get; set; }

            public virtual string ProtectedStaticGetSetReferenceType { get; set; }

            public virtual string PrivateStaticGetSetReferenceType { get; set; }

            // *

            public virtual string PublicGetReferenceType { get; }

            public virtual string InternalGetReferenceType { get; }

            public virtual string ProtectedGetReferenceType { get; }

            public virtual string PrivateGetReferenceType { get; }

            // *

            public virtual string PublicGetSetReferenceType { get; set; }

            public virtual string InternalGetSetReferenceType { get; set; }

            public virtual string ProtectedGetSetReferenceType { get; set; }

            public virtual string PrivateGetSetReferenceType { get; set; }

            // *

            public virtual string this[string index]
            {
                get => default;
                set { }
            }
        }
    }
}
