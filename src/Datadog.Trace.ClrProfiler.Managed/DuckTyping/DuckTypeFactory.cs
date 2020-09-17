using System;

namespace Datadog.Trace.ClrProfiler.DuckTyping
{
    /// <summary>
    /// Duck type factory
    /// </summary>
    public readonly struct DuckTypeFactory
    {
        private readonly Type _proxyType;

        internal DuckTypeFactory(Type proxyType)
        {
            _proxyType = proxyType;
        }

        /// <summary>
        /// Create duck type proxy instance
        /// </summary>
        /// <param name="instance">Object instance</param>
        /// <returns>Duck type proxy instance</returns>
        public IDuckType Create(object instance)
        {
            return (IDuckType)Activator.CreateInstance(_proxyType, instance);
        }
    }

    /// <summary>
    /// Duck type factory
    /// </summary>
    /// <typeparam name="T">Proxy type</typeparam>
    public readonly struct DuckTypeFactory<T>
        where T : class
    {
        private readonly Type _proxyType;

        internal DuckTypeFactory(Type proxyType)
        {
            _proxyType = proxyType;
        }

        /// <summary>
        /// Create duck type proxy instance
        /// </summary>
        /// <param name="instance">Object instance</param>
        /// <returns>Duck type proxy instance</returns>
        public T Create(object instance)
        {
            var inst = (IDuckType)Activator.CreateInstance(_proxyType, instance);
            return inst as T;
        }
    }
}
