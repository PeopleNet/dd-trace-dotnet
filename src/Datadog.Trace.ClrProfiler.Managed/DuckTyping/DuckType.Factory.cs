using System;

namespace Datadog.Trace.ClrProfiler.DuckTyping
{
    /// <summary>
    /// Duck Type
    /// </summary>
    public static partial class DuckType
    {
        /// <summary>
        /// Gets a ducktype factory for a proxy type and target type
        /// </summary>
        /// <param name="proxyType">Proxy type</param>
        /// <param name="targetType">Target type</param>
        /// <returns>Duck type factory</returns>
        public static DuckTypeFactory GetFactoryFor(Type proxyType, Type targetType)
        {
            CreateTypeResult result = GetOrCreateProxyType(proxyType, targetType);
            result.ExceptionInfo?.Throw();
            return new DuckTypeFactory(result.ProxyType);
        }

        /// <summary>
        /// Gets a ducktype factory for a proxy type and target type
        /// </summary>
        /// <param name="targetType">Type of instance</param>
        /// <typeparam name="T">Type of Proxy</typeparam>
        /// <returns>Duck Type factory</returns>
        public static DuckTypeFactory<T> GetFactoryFor<T>(Type targetType)
            where T : class
        {
            CreateTypeResult result = GetOrCreateProxyType(typeof(T), targetType);
            result.ExceptionInfo?.Throw();
            return new DuckTypeFactory<T>(result.ProxyType);
        }
    }
}
