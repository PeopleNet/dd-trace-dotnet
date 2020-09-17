using System;
using System.ComponentModel;
using System.Reflection.Emit;

namespace Datadog.Trace.ClrProfiler.CallTarget.DuckTyping
{
    /// <summary>
    /// Duck Type
    /// </summary>
    public static partial class DuckType
    {
        /// <summary>
        /// Checks and ensures the arguments for the Create methods
        /// </summary>
        /// <param name="proxyType">Duck type</param>
        /// <param name="instance">Instance value</param>
        /// <exception cref="ArgumentNullException">If the duck type or the instance value is null</exception>
        private static void EnsureArguments(Type proxyType, object instance)
        {
            if (proxyType is null)
            {
                throw new ArgumentNullException(nameof(proxyType), "The proxy type can't be null");
            }

            if (instance is null)
            {
                throw new ArgumentNullException(nameof(instance), "The object instance can't be null");
            }

            if (!proxyType.IsPublic && !proxyType.IsNestedPublic)
            {
                throw new DuckTypeTypeIsNotPublicException(proxyType, nameof(proxyType));
            }
        }

        /// <summary>
        /// Gets the DuckType value for a class DuckType chaining value
        /// </summary>
        /// <param name="originalValue">Original obscure value</param>
        /// <param name="proxyType">Proxy type</param>
        /// <param name="field">IDuckTypeClass field if the proxyType is a class</param>
        /// <returns>IDuckType instance</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static IDuckType GetClassDuckTypeChainningValue(object originalValue, Type proxyType, ref IDuckTypeClass field)
        {
            if (originalValue is null)
            {
                field?.SetInstance(null);
                return null;
            }

            var valueType = originalValue.GetType();
            if (field is null || field.Type != valueType)
            {
                CreateTypeResult result = GetOrCreateProxyType(proxyType, valueType);
                result.ExceptionInfo?.Throw();
                field = (IDuckTypeClass)Activator.CreateInstance(result.ProxyType, originalValue);
            }
            else
            {
                field.SetInstance(originalValue);
            }

            return field;
        }

        /// <summary>
        /// Gets the DuckType value for a struct DuckType chaining value
        /// </summary>
        /// <param name="originalValue">Original obscure value</param>
        /// <param name="proxyType">Proxy type</param>
        /// <returns>IDuckType instance</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static IDuckType GetStructDuckTypeChainningValue(object originalValue, Type proxyType)
        {
            if (originalValue is null)
            {
                return null;
            }

            CreateTypeResult result = GetOrCreateProxyType(proxyType, originalValue.GetType());
            result.ExceptionInfo?.Throw();
            return (IDuckType)Activator.CreateInstance(result.ProxyType, originalValue);
        }

        /// <summary>
        /// Set inner DuckType
        /// </summary>
        /// <param name="field">Field reference</param>
        /// <param name="value">Proxy type instance</param>
        /// <returns>Property value</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static object SetInnerDuckType(ref IDuckTypeClass field, IDuckTypeClass value)
        {
            field = value;
            return field?.Instance;
        }

        internal static TProxyInstance CreateProxyTypeInstance<TProxyInstance>(object value)
        {
            return ProxyActivator<TProxyInstance>.CreateInstance(value);
        }

        private readonly struct InstanceWrapper
        {
            private readonly object _currentInstance;

            public InstanceWrapper(object instance)
            {
                _currentInstance = instance;
            }
        }

        private static class ProxyActivator<TProxy>
        {
            private static Func<InstanceWrapper, TProxy> _converter;

            static ProxyActivator()
            {
                DynamicMethod converterMethod = new DynamicMethod($"WrapperConverter<{typeof(TProxy).Name}>._converter", typeof(TProxy), new[] { typeof(InstanceWrapper) });
                ILGenerator il = converterMethod.GetILGenerator();
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ret);
                _converter = (Func<InstanceWrapper, TProxy>)converterMethod.CreateDelegate(typeof(Func<InstanceWrapper, TProxy>));
            }

            public static TProxy CreateInstance(object instance)
            {
                if (typeof(TProxy).IsValueType)
                {
                    return _converter(new InstanceWrapper(instance));
                }
                else
                {
                    return (TProxy)Activator.CreateInstance(typeof(TProxy), instance);
                }
            }
        }
    }
}
