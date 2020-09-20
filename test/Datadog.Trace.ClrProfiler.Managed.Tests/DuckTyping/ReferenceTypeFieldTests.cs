using System.Collections.Generic;
using Datadog.Trace.ClrProfiler.DuckTyping;
using Xunit;

#pragma warning disable SA1201 // Elements must appear in the correct order

namespace Datadog.Trace.ClrProfiler.Managed.Tests.DuckTyping
{
    public class ReferenceTypeFieldTests
    {
        public static IEnumerable<object[]> Data()
        {
            return new[]
            {
                new object[] { ObscureObject.GetFieldPublicObject() },
                new object[] { ObscureObject.GetFieldInternalObject() },
                new object[] { ObscureObject.GetFieldPrivateObject() },
            };
        }

        [Theory]
        [MemberData(nameof(Data))]
        public void StaticReadonlyFieldsSetException(object obscureObject)
        {
            Assert.Throws<DuckTypeFieldIsReadonlyException>(() =>
            {
                obscureObject.As<IObscureStaticReadonlyErrorDuckType>();
            });
        }

        [Theory]
        [MemberData(nameof(Data))]
        public void ReadonlyFieldsSetException(object obscureObject)
        {
            Assert.Throws<DuckTypeFieldIsReadonlyException>(() =>
            {
                obscureObject.As<IObscureReadonlyErrorDuckType>();
            });
        }

        [Theory]
        [MemberData(nameof(Data))]
        public void StaticReadonlyFields(object obscureObject)
        {
            var duckInterface = obscureObject.As<IObscureDuckType>();
            var duckAbstract = obscureObject.As<ObscureDuckTypeAbstractClass>();
            var duckVirtual = obscureObject.As<ObscureDuckType>();

            // *
            Assert.Equal("10", duckInterface.PublicStaticReadonlyReferenceTypeField);
            Assert.Equal("10", duckAbstract.PublicStaticReadonlyReferenceTypeField);
            Assert.Equal("10", duckVirtual.PublicStaticReadonlyReferenceTypeField);

            // *
            Assert.Equal("11", duckInterface.InternalStaticReadonlyReferenceTypeField);
            Assert.Equal("11", duckAbstract.InternalStaticReadonlyReferenceTypeField);
            Assert.Equal("11", duckVirtual.InternalStaticReadonlyReferenceTypeField);

            // *
            Assert.Equal("12", duckInterface.ProtectedStaticReadonlyReferenceTypeField);
            Assert.Equal("12", duckAbstract.ProtectedStaticReadonlyReferenceTypeField);
            Assert.Equal("12", duckVirtual.ProtectedStaticReadonlyReferenceTypeField);

            // *
            Assert.Equal("13", duckInterface.PrivateStaticReadonlyReferenceTypeField);
            Assert.Equal("13", duckAbstract.PrivateStaticReadonlyReferenceTypeField);
            Assert.Equal("13", duckVirtual.PrivateStaticReadonlyReferenceTypeField);
        }

        [Theory]
        [MemberData(nameof(Data))]
        public void StaticFields(object obscureObject)
        {
            var duckInterface = obscureObject.As<IObscureDuckType>();
            var duckAbstract = obscureObject.As<ObscureDuckTypeAbstractClass>();
            var duckVirtual = obscureObject.As<ObscureDuckType>();

            Assert.Equal("20", duckInterface.PublicStaticReferenceTypeField);
            Assert.Equal("20", duckAbstract.PublicStaticReferenceTypeField);
            Assert.Equal("20", duckVirtual.PublicStaticReferenceTypeField);

            duckInterface.PublicStaticReferenceTypeField = "42";
            Assert.Equal("42", duckInterface.PublicStaticReferenceTypeField);
            Assert.Equal("42", duckAbstract.PublicStaticReferenceTypeField);
            Assert.Equal("42", duckVirtual.PublicStaticReferenceTypeField);

            duckAbstract.PublicStaticReferenceTypeField = "50";
            Assert.Equal("50", duckInterface.PublicStaticReferenceTypeField);
            Assert.Equal("50", duckAbstract.PublicStaticReferenceTypeField);
            Assert.Equal("50", duckVirtual.PublicStaticReferenceTypeField);

            duckVirtual.PublicStaticReferenceTypeField = "60";
            Assert.Equal("60", duckInterface.PublicStaticReferenceTypeField);
            Assert.Equal("60", duckAbstract.PublicStaticReferenceTypeField);
            Assert.Equal("60", duckVirtual.PublicStaticReferenceTypeField);

            // *

            Assert.Equal("21", duckInterface.InternalStaticReferenceTypeField);
            Assert.Equal("21", duckAbstract.InternalStaticReferenceTypeField);
            Assert.Equal("21", duckVirtual.InternalStaticReferenceTypeField);

            duckInterface.InternalStaticReferenceTypeField = "42";
            Assert.Equal("42", duckInterface.InternalStaticReferenceTypeField);
            Assert.Equal("42", duckAbstract.InternalStaticReferenceTypeField);
            Assert.Equal("42", duckVirtual.InternalStaticReferenceTypeField);

            duckAbstract.InternalStaticReferenceTypeField = "50";
            Assert.Equal("50", duckInterface.InternalStaticReferenceTypeField);
            Assert.Equal("50", duckAbstract.InternalStaticReferenceTypeField);
            Assert.Equal("50", duckVirtual.InternalStaticReferenceTypeField);

            duckVirtual.InternalStaticReferenceTypeField = "60";
            Assert.Equal("60", duckInterface.InternalStaticReferenceTypeField);
            Assert.Equal("60", duckAbstract.InternalStaticReferenceTypeField);
            Assert.Equal("60", duckVirtual.InternalStaticReferenceTypeField);

            // *

            Assert.Equal("22", duckInterface.ProtectedStaticReferenceTypeField);
            Assert.Equal("22", duckAbstract.ProtectedStaticReferenceTypeField);
            Assert.Equal("22", duckVirtual.ProtectedStaticReferenceTypeField);

            duckInterface.ProtectedStaticReferenceTypeField = "42";
            Assert.Equal("42", duckInterface.ProtectedStaticReferenceTypeField);
            Assert.Equal("42", duckAbstract.ProtectedStaticReferenceTypeField);
            Assert.Equal("42", duckVirtual.ProtectedStaticReferenceTypeField);

            duckAbstract.ProtectedStaticReferenceTypeField = "50";
            Assert.Equal("50", duckInterface.ProtectedStaticReferenceTypeField);
            Assert.Equal("50", duckAbstract.ProtectedStaticReferenceTypeField);
            Assert.Equal("50", duckVirtual.ProtectedStaticReferenceTypeField);

            duckVirtual.ProtectedStaticReferenceTypeField = "60";
            Assert.Equal("60", duckInterface.ProtectedStaticReferenceTypeField);
            Assert.Equal("60", duckAbstract.ProtectedStaticReferenceTypeField);
            Assert.Equal("60", duckVirtual.ProtectedStaticReferenceTypeField);

            // *

            Assert.Equal("23", duckInterface.PrivateStaticReferenceTypeField);
            Assert.Equal("23", duckAbstract.PrivateStaticReferenceTypeField);
            Assert.Equal("23", duckVirtual.PrivateStaticReferenceTypeField);

            duckInterface.PrivateStaticReferenceTypeField = "42";
            Assert.Equal("42", duckInterface.PrivateStaticReferenceTypeField);
            Assert.Equal("42", duckAbstract.PrivateStaticReferenceTypeField);
            Assert.Equal("42", duckVirtual.PrivateStaticReferenceTypeField);

            duckAbstract.PrivateStaticReferenceTypeField = "50";
            Assert.Equal("50", duckInterface.PrivateStaticReferenceTypeField);
            Assert.Equal("50", duckAbstract.PrivateStaticReferenceTypeField);
            Assert.Equal("50", duckVirtual.PrivateStaticReferenceTypeField);

            duckVirtual.PrivateStaticReferenceTypeField = "60";
            Assert.Equal("60", duckInterface.PrivateStaticReferenceTypeField);
            Assert.Equal("60", duckAbstract.PrivateStaticReferenceTypeField);
            Assert.Equal("60", duckVirtual.PrivateStaticReferenceTypeField);
        }

        [Theory]
        [MemberData(nameof(Data))]
        public void ReadonlyFields(object obscureObject)
        {
            var duckInterface = obscureObject.As<IObscureDuckType>();
            var duckAbstract = obscureObject.As<ObscureDuckTypeAbstractClass>();
            var duckVirtual = obscureObject.As<ObscureDuckType>();

            // *
            Assert.Equal("30", duckInterface.PublicReadonlyReferenceTypeField);
            Assert.Equal("30", duckAbstract.PublicReadonlyReferenceTypeField);
            Assert.Equal("30", duckVirtual.PublicReadonlyReferenceTypeField);

            // *
            Assert.Equal("31", duckInterface.InternalReadonlyReferenceTypeField);
            Assert.Equal("31", duckAbstract.InternalReadonlyReferenceTypeField);
            Assert.Equal("31", duckVirtual.InternalReadonlyReferenceTypeField);

            // *
            Assert.Equal("32", duckInterface.ProtectedReadonlyReferenceTypeField);
            Assert.Equal("32", duckAbstract.ProtectedReadonlyReferenceTypeField);
            Assert.Equal("32", duckVirtual.ProtectedReadonlyReferenceTypeField);

            // *
            Assert.Equal("33", duckInterface.PrivateReadonlyReferenceTypeField);
            Assert.Equal("33", duckAbstract.PrivateReadonlyReferenceTypeField);
            Assert.Equal("33", duckVirtual.PrivateReadonlyReferenceTypeField);
        }

        [Theory]
        [MemberData(nameof(Data))]
        public void Fields(object obscureObject)
        {
            var duckInterface = obscureObject.As<IObscureDuckType>();
            var duckAbstract = obscureObject.As<ObscureDuckTypeAbstractClass>();
            var duckVirtual = obscureObject.As<ObscureDuckType>();

            Assert.Equal("40", duckInterface.PublicReferenceTypeField);
            Assert.Equal("40", duckAbstract.PublicReferenceTypeField);
            Assert.Equal("40", duckVirtual.PublicReferenceTypeField);

            duckInterface.PublicReferenceTypeField = "42";
            Assert.Equal("42", duckInterface.PublicReferenceTypeField);
            Assert.Equal("42", duckAbstract.PublicReferenceTypeField);
            Assert.Equal("42", duckVirtual.PublicReferenceTypeField);

            duckAbstract.PublicReferenceTypeField = "50";
            Assert.Equal("50", duckInterface.PublicReferenceTypeField);
            Assert.Equal("50", duckAbstract.PublicReferenceTypeField);
            Assert.Equal("50", duckVirtual.PublicReferenceTypeField);

            duckVirtual.PublicReferenceTypeField = "60";
            Assert.Equal("60", duckInterface.PublicReferenceTypeField);
            Assert.Equal("60", duckAbstract.PublicReferenceTypeField);
            Assert.Equal("60", duckVirtual.PublicReferenceTypeField);

            // *

            Assert.Equal("41", duckInterface.InternalReferenceTypeField);
            Assert.Equal("41", duckAbstract.InternalReferenceTypeField);
            Assert.Equal("41", duckVirtual.InternalReferenceTypeField);

            duckInterface.InternalReferenceTypeField = "42";
            Assert.Equal("42", duckInterface.InternalReferenceTypeField);
            Assert.Equal("42", duckAbstract.InternalReferenceTypeField);
            Assert.Equal("42", duckVirtual.InternalReferenceTypeField);

            duckAbstract.InternalReferenceTypeField = "50";
            Assert.Equal("50", duckInterface.InternalReferenceTypeField);
            Assert.Equal("50", duckAbstract.InternalReferenceTypeField);
            Assert.Equal("50", duckVirtual.InternalReferenceTypeField);

            duckVirtual.InternalReferenceTypeField = "60";
            Assert.Equal("60", duckInterface.InternalReferenceTypeField);
            Assert.Equal("60", duckAbstract.InternalReferenceTypeField);
            Assert.Equal("60", duckVirtual.InternalReferenceTypeField);

            // *

            Assert.Equal("42", duckInterface.ProtectedReferenceTypeField);
            Assert.Equal("42", duckAbstract.ProtectedReferenceTypeField);
            Assert.Equal("42", duckVirtual.ProtectedReferenceTypeField);

            duckInterface.ProtectedReferenceTypeField = "45";
            Assert.Equal("45", duckInterface.ProtectedReferenceTypeField);
            Assert.Equal("45", duckAbstract.ProtectedReferenceTypeField);
            Assert.Equal("45", duckVirtual.ProtectedReferenceTypeField);

            duckAbstract.ProtectedReferenceTypeField = "50";
            Assert.Equal("50", duckInterface.ProtectedReferenceTypeField);
            Assert.Equal("50", duckAbstract.ProtectedReferenceTypeField);
            Assert.Equal("50", duckVirtual.ProtectedReferenceTypeField);

            duckVirtual.ProtectedReferenceTypeField = "60";
            Assert.Equal("60", duckInterface.ProtectedReferenceTypeField);
            Assert.Equal("60", duckAbstract.ProtectedReferenceTypeField);
            Assert.Equal("60", duckVirtual.ProtectedReferenceTypeField);

            // *

            Assert.Equal("43", duckInterface.PrivateReferenceTypeField);
            Assert.Equal("43", duckAbstract.PrivateReferenceTypeField);
            Assert.Equal("43", duckVirtual.PrivateReferenceTypeField);

            duckInterface.PrivateReferenceTypeField = "42";
            Assert.Equal("42", duckInterface.PrivateReferenceTypeField);
            Assert.Equal("42", duckAbstract.PrivateReferenceTypeField);
            Assert.Equal("42", duckVirtual.PrivateReferenceTypeField);

            duckAbstract.PrivateReferenceTypeField = "50";
            Assert.Equal("50", duckInterface.PrivateReferenceTypeField);
            Assert.Equal("50", duckAbstract.PrivateReferenceTypeField);
            Assert.Equal("50", duckVirtual.PrivateReferenceTypeField);

            duckVirtual.PrivateReferenceTypeField = "60";
            Assert.Equal("60", duckInterface.PrivateReferenceTypeField);
            Assert.Equal("60", duckAbstract.PrivateReferenceTypeField);
            Assert.Equal("60", duckVirtual.PrivateReferenceTypeField);
        }

        public interface IObscureDuckType
        {
            [Duck(Name = "_publicStaticReadonlyReferenceTypeField", Kind = DuckKind.Field)]
            string PublicStaticReadonlyReferenceTypeField { get; }

            [Duck(Name = "_internalStaticReadonlyReferenceTypeField", Kind = DuckKind.Field)]
            string InternalStaticReadonlyReferenceTypeField { get; }

            [Duck(Name = "_protectedStaticReadonlyReferenceTypeField", Kind = DuckKind.Field)]
            string ProtectedStaticReadonlyReferenceTypeField { get; }

            [Duck(Name = "_privateStaticReadonlyReferenceTypeField", Kind = DuckKind.Field)]
            string PrivateStaticReadonlyReferenceTypeField { get; }

            // *

            [Duck(Name = "_publicStaticReferenceTypeField", Kind = DuckKind.Field)]
            string PublicStaticReferenceTypeField { get; set; }

            [Duck(Name = "_internalStaticReferenceTypeField", Kind = DuckKind.Field)]
            string InternalStaticReferenceTypeField { get; set; }

            [Duck(Name = "_protectedStaticReferenceTypeField", Kind = DuckKind.Field)]
            string ProtectedStaticReferenceTypeField { get; set; }

            [Duck(Name = "_privateStaticReferenceTypeField", Kind = DuckKind.Field)]
            string PrivateStaticReferenceTypeField { get; set; }

            // *

            [Duck(Name = "_publicReadonlyReferenceTypeField", Kind = DuckKind.Field)]
            string PublicReadonlyReferenceTypeField { get; }

            [Duck(Name = "_internalReadonlyReferenceTypeField", Kind = DuckKind.Field)]
            string InternalReadonlyReferenceTypeField { get; }

            [Duck(Name = "_protectedReadonlyReferenceTypeField", Kind = DuckKind.Field)]
            string ProtectedReadonlyReferenceTypeField { get; }

            [Duck(Name = "_privateReadonlyReferenceTypeField", Kind = DuckKind.Field)]
            string PrivateReadonlyReferenceTypeField { get; }

            // *

            [Duck(Name = "_publicReferenceTypeField", Kind = DuckKind.Field)]
            string PublicReferenceTypeField { get; set; }

            [Duck(Name = "_internalReferenceTypeField", Kind = DuckKind.Field)]
            string InternalReferenceTypeField { get; set; }

            [Duck(Name = "_protectedReferenceTypeField", Kind = DuckKind.Field)]
            string ProtectedReferenceTypeField { get; set; }

            [Duck(Name = "_privateReferenceTypeField", Kind = DuckKind.Field)]
            string PrivateReferenceTypeField { get; set; }
        }

        public interface IObscureStaticReadonlyErrorDuckType
        {
            [Duck(Name = "_publicStaticReadonlyReferenceTypeField", Kind = DuckKind.Field)]
            string PublicStaticReadonlyReferenceTypeField { get; set; }
        }

        public interface IObscureReadonlyErrorDuckType
        {
            [Duck(Name = "_publicReadonlyReferenceTypeField", Kind = DuckKind.Field)]
            string PublicReadonlyReferenceTypeField { get; set; }
        }

        public abstract class ObscureDuckTypeAbstractClass
        {
            [Duck(Name = "_publicStaticReadonlyReferenceTypeField", Kind = DuckKind.Field)]
            public abstract string PublicStaticReadonlyReferenceTypeField { get; }

            [Duck(Name = "_internalStaticReadonlyReferenceTypeField", Kind = DuckKind.Field)]
            public abstract string InternalStaticReadonlyReferenceTypeField { get; }

            [Duck(Name = "_protectedStaticReadonlyReferenceTypeField", Kind = DuckKind.Field)]
            public abstract string ProtectedStaticReadonlyReferenceTypeField { get; }

            [Duck(Name = "_privateStaticReadonlyReferenceTypeField", Kind = DuckKind.Field)]
            public abstract string PrivateStaticReadonlyReferenceTypeField { get; }

            // *

            [Duck(Name = "_publicStaticReferenceTypeField", Kind = DuckKind.Field)]
            public abstract string PublicStaticReferenceTypeField { get; set; }

            [Duck(Name = "_internalStaticReferenceTypeField", Kind = DuckKind.Field)]
            public abstract string InternalStaticReferenceTypeField { get; set; }

            [Duck(Name = "_protectedStaticReferenceTypeField", Kind = DuckKind.Field)]
            public abstract string ProtectedStaticReferenceTypeField { get; set; }

            [Duck(Name = "_privateStaticReferenceTypeField", Kind = DuckKind.Field)]
            public abstract string PrivateStaticReferenceTypeField { get; set; }

            // *

            [Duck(Name = "_publicReadonlyReferenceTypeField", Kind = DuckKind.Field)]
            public abstract string PublicReadonlyReferenceTypeField { get; }

            [Duck(Name = "_internalReadonlyReferenceTypeField", Kind = DuckKind.Field)]
            public abstract string InternalReadonlyReferenceTypeField { get; }

            [Duck(Name = "_protectedReadonlyReferenceTypeField", Kind = DuckKind.Field)]
            public abstract string ProtectedReadonlyReferenceTypeField { get; }

            [Duck(Name = "_privateReadonlyReferenceTypeField", Kind = DuckKind.Field)]
            public abstract string PrivateReadonlyReferenceTypeField { get; }

            // *

            [Duck(Name = "_publicReferenceTypeField", Kind = DuckKind.Field)]
            public abstract string PublicReferenceTypeField { get; set; }

            [Duck(Name = "_internalReferenceTypeField", Kind = DuckKind.Field)]
            public abstract string InternalReferenceTypeField { get; set; }

            [Duck(Name = "_protectedReferenceTypeField", Kind = DuckKind.Field)]
            public abstract string ProtectedReferenceTypeField { get; set; }

            [Duck(Name = "_privateReferenceTypeField", Kind = DuckKind.Field)]
            public abstract string PrivateReferenceTypeField { get; set; }
        }

        public class ObscureDuckType
        {
            [Duck(Name = "_publicStaticReadonlyReferenceTypeField", Kind = DuckKind.Field)]
            public virtual string PublicStaticReadonlyReferenceTypeField { get; }

            [Duck(Name = "_internalStaticReadonlyReferenceTypeField", Kind = DuckKind.Field)]
            public virtual string InternalStaticReadonlyReferenceTypeField { get; }

            [Duck(Name = "_protectedStaticReadonlyReferenceTypeField", Kind = DuckKind.Field)]
            public virtual string ProtectedStaticReadonlyReferenceTypeField { get; }

            [Duck(Name = "_privateStaticReadonlyReferenceTypeField", Kind = DuckKind.Field)]
            public virtual string PrivateStaticReadonlyReferenceTypeField { get; }

            // *

            [Duck(Name = "_publicStaticReferenceTypeField", Kind = DuckKind.Field)]
            public virtual string PublicStaticReferenceTypeField { get; set; }

            [Duck(Name = "_internalStaticReferenceTypeField", Kind = DuckKind.Field)]
            public virtual string InternalStaticReferenceTypeField { get; set; }

            [Duck(Name = "_protectedStaticReferenceTypeField", Kind = DuckKind.Field)]
            public virtual string ProtectedStaticReferenceTypeField { get; set; }

            [Duck(Name = "_privateStaticReferenceTypeField", Kind = DuckKind.Field)]
            public virtual string PrivateStaticReferenceTypeField { get; set; }

            // *

            [Duck(Name = "_publicReadonlyReferenceTypeField", Kind = DuckKind.Field)]
            public virtual string PublicReadonlyReferenceTypeField { get; }

            [Duck(Name = "_internalReadonlyReferenceTypeField", Kind = DuckKind.Field)]
            public virtual string InternalReadonlyReferenceTypeField { get; }

            [Duck(Name = "_protectedReadonlyReferenceTypeField", Kind = DuckKind.Field)]
            public virtual string ProtectedReadonlyReferenceTypeField { get; }

            [Duck(Name = "_privateReadonlyReferenceTypeField", Kind = DuckKind.Field)]
            public virtual string PrivateReadonlyReferenceTypeField { get; }

            // *

            [Duck(Name = "_publicReferenceTypeField", Kind = DuckKind.Field)]
            public virtual string PublicReferenceTypeField { get; set; }

            [Duck(Name = "_internalReferenceTypeField", Kind = DuckKind.Field)]
            public virtual string InternalReferenceTypeField { get; set; }

            [Duck(Name = "_protectedReferenceTypeField", Kind = DuckKind.Field)]
            public virtual string ProtectedReferenceTypeField { get; set; }

            [Duck(Name = "_privateReferenceTypeField", Kind = DuckKind.Field)]
            public virtual string PrivateReferenceTypeField { get; set; }
        }
    }
}
