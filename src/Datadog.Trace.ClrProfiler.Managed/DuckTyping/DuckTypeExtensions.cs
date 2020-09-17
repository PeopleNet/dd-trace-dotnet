using System;

namespace Datadog.Trace.ClrProfiler.DuckTyping
{
    /// <summary>
    /// Duck type extensions
    /// </summary>
    public static class DuckTypeExtensions
    {
        /// <summary>
        /// Gets the duck type factory for the type implementing a base class or interface T
        /// </summary>
        /// <param name="targetType">Target type</param>
        /// <typeparam name="T">Proxy type</typeparam>
        /// <returns>DuckTypeFactory instance</returns>
        public static DuckTypeFactory<T> GetDuckTypeFactory<T>(this Type targetType)
            where T : class
            => DuckType.GetFactoryFor<T>(targetType);

        /// <summary>
        /// Gets the duck type factory for the type implementing a base class or interface T
        /// </summary>
        /// <param name="targetType">Target type</param>
        /// <param name="proxyType">Proxy type</param>
        /// <returns>DuckTypeFactory instance</returns>
        public static DuckTypeFactory GetDuckTypeFactory(this Type targetType, Type proxyType)
            => DuckType.GetFactoryFor(proxyType, targetType);

        /// <summary>
        /// Gets the duck type instance for the object implementing a base class or interface T
        /// </summary>
        /// <param name="instance">Object instance</param>
        /// <typeparam name="T">Target type</typeparam>
        /// <returns>DuckType instance</returns>
        public static T As<T>(this object instance)
            where T : class
            => DuckType.Create<T>(instance);

        /// <summary>
        /// Gets the duck type instance for the object implementing a base class or interface T
        /// </summary>
        /// <param name="instance">Object instance</param>
        /// <param name="targetType">Target type</param>
        /// <returns>DuckType instance</returns>
        public static object As(this object instance, Type targetType)
            => DuckType.Create(targetType, instance);
    }
}
