using System.Collections.Generic;
using Datadog.Trace.ClrProfiler.DuckTyping;
using Xunit;

#pragma warning disable SA1201 // Elements must appear in the correct order

namespace Datadog.Trace.ClrProfiler.Managed.Tests.DuckTyping
{
    public class TypeChainingFieldTests
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

            Assert.Equal(42, duckInterface.PublicStaticReadonlySelfTypeField.MagicNumber);
            Assert.Equal(42, duckAbstract.PublicStaticReadonlySelfTypeField.MagicNumber);
            Assert.Equal(42, duckVirtual.PublicStaticReadonlySelfTypeField.MagicNumber);

            Assert.Equal(ObscureObject.DummyFieldObject.Default, ((IDuckType)duckInterface.PublicStaticReadonlySelfTypeField).Instance);
            Assert.Equal(ObscureObject.DummyFieldObject.Default, ((IDuckType)duckAbstract.PublicStaticReadonlySelfTypeField).Instance);
            Assert.Equal(ObscureObject.DummyFieldObject.Default, ((IDuckType)duckVirtual.PublicStaticReadonlySelfTypeField).Instance);

            // *

            Assert.Equal(42, duckInterface.InternalStaticReadonlySelfTypeField.MagicNumber);
            Assert.Equal(42, duckAbstract.InternalStaticReadonlySelfTypeField.MagicNumber);
            Assert.Equal(42, duckVirtual.InternalStaticReadonlySelfTypeField.MagicNumber);

            Assert.Equal(ObscureObject.DummyFieldObject.Default, ((IDuckType)duckInterface.InternalStaticReadonlySelfTypeField).Instance);
            Assert.Equal(ObscureObject.DummyFieldObject.Default, ((IDuckType)duckAbstract.InternalStaticReadonlySelfTypeField).Instance);
            Assert.Equal(ObscureObject.DummyFieldObject.Default, ((IDuckType)duckVirtual.InternalStaticReadonlySelfTypeField).Instance);

            // *

            Assert.Equal(42, duckInterface.ProtectedStaticReadonlySelfTypeField.MagicNumber);
            Assert.Equal(42, duckAbstract.ProtectedStaticReadonlySelfTypeField.MagicNumber);
            Assert.Equal(42, duckVirtual.ProtectedStaticReadonlySelfTypeField.MagicNumber);

            Assert.Equal(ObscureObject.DummyFieldObject.Default, ((IDuckType)duckInterface.ProtectedStaticReadonlySelfTypeField).Instance);
            Assert.Equal(ObscureObject.DummyFieldObject.Default, ((IDuckType)duckAbstract.ProtectedStaticReadonlySelfTypeField).Instance);
            Assert.Equal(ObscureObject.DummyFieldObject.Default, ((IDuckType)duckVirtual.ProtectedStaticReadonlySelfTypeField).Instance);

            // *

            Assert.Equal(42, duckInterface.PrivateStaticReadonlySelfTypeField.MagicNumber);
            Assert.Equal(42, duckAbstract.PrivateStaticReadonlySelfTypeField.MagicNumber);
            Assert.Equal(42, duckVirtual.PrivateStaticReadonlySelfTypeField.MagicNumber);

            Assert.Equal(ObscureObject.DummyFieldObject.Default, ((IDuckType)duckInterface.PrivateStaticReadonlySelfTypeField).Instance);
            Assert.Equal(ObscureObject.DummyFieldObject.Default, ((IDuckType)duckAbstract.PrivateStaticReadonlySelfTypeField).Instance);
            Assert.Equal(ObscureObject.DummyFieldObject.Default, ((IDuckType)duckVirtual.PrivateStaticReadonlySelfTypeField).Instance);
        }

        [Theory]
        [MemberData(nameof(Data))]
        public void StaticFields(object obscureObject)
        {
            var duckInterface = obscureObject.As<IObscureDuckType>();
            var duckAbstract = obscureObject.As<ObscureDuckTypeAbstractClass>();
            var duckVirtual = obscureObject.As<ObscureDuckType>();

            IDummyFieldObject newDummy = null;

            // *
            newDummy = (new ObscureObject.DummyFieldObject { MagicNumber = 42 }).As<IDummyFieldObject>();
            duckInterface.PublicStaticSelfTypeField = newDummy;

            Assert.Equal(42, duckInterface.PublicStaticSelfTypeField.MagicNumber);
            Assert.Equal(42, duckAbstract.PublicStaticSelfTypeField.MagicNumber);
            Assert.Equal(42, duckVirtual.PublicStaticSelfTypeField.MagicNumber);

            // *
            newDummy = (new ObscureObject.DummyFieldObject { MagicNumber = 52 }).As<IDummyFieldObject>();
            duckInterface.InternalStaticSelfTypeField = newDummy;

            Assert.Equal(52, duckInterface.InternalStaticSelfTypeField.MagicNumber);
            Assert.Equal(52, duckAbstract.InternalStaticSelfTypeField.MagicNumber);
            Assert.Equal(52, duckVirtual.InternalStaticSelfTypeField.MagicNumber);

            // *
            newDummy = (new ObscureObject.DummyFieldObject { MagicNumber = 62 }).As<IDummyFieldObject>();
            duckAbstract.ProtectedStaticSelfTypeField = newDummy;

            Assert.Equal(62, duckInterface.ProtectedStaticSelfTypeField.MagicNumber);
            Assert.Equal(62, duckAbstract.ProtectedStaticSelfTypeField.MagicNumber);
            Assert.Equal(62, duckVirtual.ProtectedStaticSelfTypeField.MagicNumber);

            // *
            newDummy = (new ObscureObject.DummyFieldObject { MagicNumber = 72 }).As<IDummyFieldObject>();
            duckAbstract.PrivateStaticSelfTypeField = newDummy;

            Assert.Equal(72, duckInterface.PrivateStaticSelfTypeField.MagicNumber);
            Assert.Equal(72, duckAbstract.PrivateStaticSelfTypeField.MagicNumber);
            Assert.Equal(72, duckVirtual.PrivateStaticSelfTypeField.MagicNumber);
        }

        [Theory]
        [MemberData(nameof(Data))]
        public void ReadonlyFields(object obscureObject)
        {
            var duckInterface = obscureObject.As<IObscureDuckType>();
            var duckAbstract = obscureObject.As<ObscureDuckTypeAbstractClass>();
            var duckVirtual = obscureObject.As<ObscureDuckType>();

            // *

            Assert.Equal(42, duckInterface.PublicReadonlySelfTypeField.MagicNumber);
            Assert.Equal(42, duckAbstract.PublicReadonlySelfTypeField.MagicNumber);
            Assert.Equal(42, duckVirtual.PublicReadonlySelfTypeField.MagicNumber);

            Assert.Equal(ObscureObject.DummyFieldObject.Default, ((IDuckType)duckInterface.PublicReadonlySelfTypeField).Instance);
            Assert.Equal(ObscureObject.DummyFieldObject.Default, ((IDuckType)duckAbstract.PublicReadonlySelfTypeField).Instance);
            Assert.Equal(ObscureObject.DummyFieldObject.Default, ((IDuckType)duckVirtual.PublicReadonlySelfTypeField).Instance);

            // *

            Assert.Equal(42, duckInterface.InternalReadonlySelfTypeField.MagicNumber);
            Assert.Equal(42, duckAbstract.InternalReadonlySelfTypeField.MagicNumber);
            Assert.Equal(42, duckVirtual.InternalReadonlySelfTypeField.MagicNumber);

            Assert.Equal(ObscureObject.DummyFieldObject.Default, ((IDuckType)duckInterface.InternalReadonlySelfTypeField).Instance);
            Assert.Equal(ObscureObject.DummyFieldObject.Default, ((IDuckType)duckAbstract.InternalReadonlySelfTypeField).Instance);
            Assert.Equal(ObscureObject.DummyFieldObject.Default, ((IDuckType)duckVirtual.InternalReadonlySelfTypeField).Instance);

            // *

            Assert.Equal(42, duckInterface.ProtectedReadonlySelfTypeField.MagicNumber);
            Assert.Equal(42, duckAbstract.ProtectedReadonlySelfTypeField.MagicNumber);
            Assert.Equal(42, duckVirtual.ProtectedReadonlySelfTypeField.MagicNumber);

            Assert.Equal(ObscureObject.DummyFieldObject.Default, ((IDuckType)duckInterface.ProtectedReadonlySelfTypeField).Instance);
            Assert.Equal(ObscureObject.DummyFieldObject.Default, ((IDuckType)duckAbstract.ProtectedReadonlySelfTypeField).Instance);
            Assert.Equal(ObscureObject.DummyFieldObject.Default, ((IDuckType)duckVirtual.ProtectedReadonlySelfTypeField).Instance);

            // *

            Assert.Equal(42, duckInterface.PrivateReadonlySelfTypeField.MagicNumber);
            Assert.Equal(42, duckAbstract.PrivateReadonlySelfTypeField.MagicNumber);
            Assert.Equal(42, duckVirtual.PrivateReadonlySelfTypeField.MagicNumber);

            Assert.Equal(ObscureObject.DummyFieldObject.Default, ((IDuckType)duckInterface.PrivateReadonlySelfTypeField).Instance);
            Assert.Equal(ObscureObject.DummyFieldObject.Default, ((IDuckType)duckAbstract.PrivateReadonlySelfTypeField).Instance);
            Assert.Equal(ObscureObject.DummyFieldObject.Default, ((IDuckType)duckVirtual.PrivateReadonlySelfTypeField).Instance);
        }

        [Theory]
        [MemberData(nameof(Data))]
        public void Fields(object obscureObject)
        {
            var duckInterface = obscureObject.As<IObscureDuckType>();
            var duckAbstract = obscureObject.As<ObscureDuckTypeAbstractClass>();
            var duckVirtual = obscureObject.As<ObscureDuckType>();

            IDummyFieldObject newDummy = null;

            // *
            newDummy = (new ObscureObject.DummyFieldObject { MagicNumber = 42 }).As<IDummyFieldObject>();
            duckInterface.PublicSelfTypeField = newDummy;

            Assert.Equal(42, duckInterface.PublicSelfTypeField.MagicNumber);
            Assert.Equal(42, duckAbstract.PublicSelfTypeField.MagicNumber);
            Assert.Equal(42, duckVirtual.PublicSelfTypeField.MagicNumber);

            // *
            newDummy = (new ObscureObject.DummyFieldObject { MagicNumber = 52 }).As<IDummyFieldObject>();
            duckInterface.InternalSelfTypeField = newDummy;

            Assert.Equal(52, duckInterface.InternalSelfTypeField.MagicNumber);
            Assert.Equal(52, duckAbstract.InternalSelfTypeField.MagicNumber);
            Assert.Equal(52, duckVirtual.InternalSelfTypeField.MagicNumber);

            // *
            newDummy = (new ObscureObject.DummyFieldObject { MagicNumber = 62 }).As<IDummyFieldObject>();
            duckInterface.ProtectedSelfTypeField = newDummy;

            Assert.Equal(62, duckInterface.ProtectedSelfTypeField.MagicNumber);
            Assert.Equal(62, duckAbstract.ProtectedSelfTypeField.MagicNumber);
            Assert.Equal(62, duckVirtual.ProtectedSelfTypeField.MagicNumber);

            // *
            newDummy = (new ObscureObject.DummyFieldObject { MagicNumber = 72 }).As<IDummyFieldObject>();
            duckInterface.PrivateSelfTypeField = newDummy;

            Assert.Equal(72, duckInterface.PrivateSelfTypeField.MagicNumber);
            Assert.Equal(72, duckAbstract.PrivateSelfTypeField.MagicNumber);
            Assert.Equal(72, duckVirtual.PrivateSelfTypeField.MagicNumber);
        }

        public interface IObscureDuckType
        {
            [Duck(Name = "_publicStaticReadonlySelfTypeField", Kind = DuckKind.Field)]
            IDummyFieldObject PublicStaticReadonlySelfTypeField { get; }

            [Duck(Name = "_internalStaticReadonlySelfTypeField", Kind = DuckKind.Field)]
            IDummyFieldObject InternalStaticReadonlySelfTypeField { get; }

            [Duck(Name = "_protectedStaticReadonlySelfTypeField", Kind = DuckKind.Field)]
            IDummyFieldObject ProtectedStaticReadonlySelfTypeField { get; }

            [Duck(Name = "_privateStaticReadonlySelfTypeField", Kind = DuckKind.Field)]
            IDummyFieldObject PrivateStaticReadonlySelfTypeField { get; }

            // *

            [Duck(Name = "_publicStaticSelfTypeField", Kind = DuckKind.Field)]
            IDummyFieldObject PublicStaticSelfTypeField { get; set; }

            [Duck(Name = "_internalStaticSelfTypeField", Kind = DuckKind.Field)]
            IDummyFieldObject InternalStaticSelfTypeField { get; set; }

            [Duck(Name = "_protectedStaticSelfTypeField", Kind = DuckKind.Field)]
            IDummyFieldObject ProtectedStaticSelfTypeField { get; set; }

            [Duck(Name = "_privateStaticSelfTypeField", Kind = DuckKind.Field)]
            IDummyFieldObject PrivateStaticSelfTypeField { get; set; }

            // *

            [Duck(Name = "_publicReadonlySelfTypeField", Kind = DuckKind.Field)]
            IDummyFieldObject PublicReadonlySelfTypeField { get; }

            [Duck(Name = "_internalReadonlySelfTypeField", Kind = DuckKind.Field)]
            IDummyFieldObject InternalReadonlySelfTypeField { get; }

            [Duck(Name = "_protectedReadonlySelfTypeField", Kind = DuckKind.Field)]
            IDummyFieldObject ProtectedReadonlySelfTypeField { get; }

            [Duck(Name = "_privateReadonlySelfTypeField", Kind = DuckKind.Field)]
            IDummyFieldObject PrivateReadonlySelfTypeField { get; }

            // *

            [Duck(Name = "_publicSelfTypeField", Kind = DuckKind.Field)]
            IDummyFieldObject PublicSelfTypeField { get; set; }

            [Duck(Name = "_internalSelfTypeField", Kind = DuckKind.Field)]
            IDummyFieldObject InternalSelfTypeField { get; set; }

            [Duck(Name = "_protectedSelfTypeField", Kind = DuckKind.Field)]
            IDummyFieldObject ProtectedSelfTypeField { get; set; }

            [Duck(Name = "_privateSelfTypeField", Kind = DuckKind.Field)]
            IDummyFieldObject PrivateSelfTypeField { get; set; }
        }

        public interface IObscureStaticReadonlyErrorDuckType
        {
            [Duck(Name = "_publicStaticReadonlySelfTypeField", Kind = DuckKind.Field)]
            IDummyFieldObject PublicStaticReadonlySelfTypeField { get; set; }
        }

        public interface IObscureReadonlyErrorDuckType
        {
            [Duck(Name = "_publicReadonlySelfTypeField", Kind = DuckKind.Field)]
            IDummyFieldObject PublicReadonlySelfTypeField { get; set; }
        }

        public abstract class ObscureDuckTypeAbstractClass
        {
            [Duck(Name = "_publicStaticReadonlySelfTypeField", Kind = DuckKind.Field)]
            public abstract IDummyFieldObject PublicStaticReadonlySelfTypeField { get; }

            [Duck(Name = "_internalStaticReadonlySelfTypeField", Kind = DuckKind.Field)]
            public abstract IDummyFieldObject InternalStaticReadonlySelfTypeField { get; }

            [Duck(Name = "_protectedStaticReadonlySelfTypeField", Kind = DuckKind.Field)]
            public abstract IDummyFieldObject ProtectedStaticReadonlySelfTypeField { get; }

            [Duck(Name = "_privateStaticReadonlySelfTypeField", Kind = DuckKind.Field)]
            public abstract IDummyFieldObject PrivateStaticReadonlySelfTypeField { get; }

            // *

            [Duck(Name = "_publicStaticSelfTypeField", Kind = DuckKind.Field)]
            public abstract IDummyFieldObject PublicStaticSelfTypeField { get; set; }

            [Duck(Name = "_internalStaticSelfTypeField", Kind = DuckKind.Field)]
            public abstract IDummyFieldObject InternalStaticSelfTypeField { get; set; }

            [Duck(Name = "_protectedStaticSelfTypeField", Kind = DuckKind.Field)]
            public abstract IDummyFieldObject ProtectedStaticSelfTypeField { get; set; }

            [Duck(Name = "_privateStaticSelfTypeField", Kind = DuckKind.Field)]
            public abstract IDummyFieldObject PrivateStaticSelfTypeField { get; set; }

            // *

            [Duck(Name = "_publicReadonlySelfTypeField", Kind = DuckKind.Field)]
            public abstract IDummyFieldObject PublicReadonlySelfTypeField { get; }

            [Duck(Name = "_internalReadonlySelfTypeField", Kind = DuckKind.Field)]
            public abstract IDummyFieldObject InternalReadonlySelfTypeField { get; }

            [Duck(Name = "_protectedReadonlySelfTypeField", Kind = DuckKind.Field)]
            public abstract IDummyFieldObject ProtectedReadonlySelfTypeField { get; }

            [Duck(Name = "_privateReadonlySelfTypeField", Kind = DuckKind.Field)]
            public abstract IDummyFieldObject PrivateReadonlySelfTypeField { get; }

            // *

            [Duck(Name = "_publicSelfTypeField", Kind = DuckKind.Field)]
            public abstract IDummyFieldObject PublicSelfTypeField { get; set; }

            [Duck(Name = "_internalSelfTypeField", Kind = DuckKind.Field)]
            public abstract IDummyFieldObject InternalSelfTypeField { get; set; }

            [Duck(Name = "_protectedSelfTypeField", Kind = DuckKind.Field)]
            public abstract IDummyFieldObject ProtectedSelfTypeField { get; set; }

            [Duck(Name = "_privateSelfTypeField", Kind = DuckKind.Field)]
            public abstract IDummyFieldObject PrivateSelfTypeField { get; set; }
        }

        public class ObscureDuckType
        {
            [Duck(Name = "_publicStaticReadonlySelfTypeField", Kind = DuckKind.Field)]
            public virtual IDummyFieldObject PublicStaticReadonlySelfTypeField { get; }

            [Duck(Name = "_internalStaticReadonlySelfTypeField", Kind = DuckKind.Field)]
            public virtual IDummyFieldObject InternalStaticReadonlySelfTypeField { get; }

            [Duck(Name = "_protectedStaticReadonlySelfTypeField", Kind = DuckKind.Field)]
            public virtual IDummyFieldObject ProtectedStaticReadonlySelfTypeField { get; }

            [Duck(Name = "_privateStaticReadonlySelfTypeField", Kind = DuckKind.Field)]
            public virtual IDummyFieldObject PrivateStaticReadonlySelfTypeField { get; }

            // *

            [Duck(Name = "_publicStaticSelfTypeField", Kind = DuckKind.Field)]
            public virtual IDummyFieldObject PublicStaticSelfTypeField { get; set; }

            [Duck(Name = "_internalStaticSelfTypeField", Kind = DuckKind.Field)]
            public virtual IDummyFieldObject InternalStaticSelfTypeField { get; set; }

            [Duck(Name = "_protectedStaticSelfTypeField", Kind = DuckKind.Field)]
            public virtual IDummyFieldObject ProtectedStaticSelfTypeField { get; set; }

            [Duck(Name = "_privateStaticSelfTypeField", Kind = DuckKind.Field)]
            public virtual IDummyFieldObject PrivateStaticSelfTypeField { get; set; }

            // *

            [Duck(Name = "_publicReadonlySelfTypeField", Kind = DuckKind.Field)]
            public virtual IDummyFieldObject PublicReadonlySelfTypeField { get; }

            [Duck(Name = "_internalReadonlySelfTypeField", Kind = DuckKind.Field)]
            public virtual IDummyFieldObject InternalReadonlySelfTypeField { get; }

            [Duck(Name = "_protectedReadonlySelfTypeField", Kind = DuckKind.Field)]
            public virtual IDummyFieldObject ProtectedReadonlySelfTypeField { get; }

            [Duck(Name = "_privateReadonlySelfTypeField", Kind = DuckKind.Field)]
            public virtual IDummyFieldObject PrivateReadonlySelfTypeField { get; }

            // *

            [Duck(Name = "_publicSelfTypeField", Kind = DuckKind.Field)]
            public virtual IDummyFieldObject PublicSelfTypeField { get; set; }

            [Duck(Name = "_internalSelfTypeField", Kind = DuckKind.Field)]
            public virtual IDummyFieldObject InternalSelfTypeField { get; set; }

            [Duck(Name = "_protectedSelfTypeField", Kind = DuckKind.Field)]
            public virtual IDummyFieldObject ProtectedSelfTypeField { get; set; }

            [Duck(Name = "_privateSelfTypeField", Kind = DuckKind.Field)]
            public virtual IDummyFieldObject PrivateSelfTypeField { get; set; }
        }

        public interface IDummyFieldObject
        {
            [Duck(Kind = DuckKind.Field)]
            int MagicNumber { get; set; }
        }
    }
}
