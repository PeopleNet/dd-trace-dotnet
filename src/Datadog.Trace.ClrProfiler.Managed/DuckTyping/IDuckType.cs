using System;

namespace Datadog.Trace.ClrProfiler.DuckTyping
{
    /// <summary>
    /// Duck type interface
    /// </summary>
    public interface IDuckType
    {
        /// <summary>
        /// Gets instance
        /// </summary>
        object Instance { get; }

        /// <summary>
        /// Gets instance Type
        /// </summary>
        Type Type { get; }

        /// <summary>
        /// Gets assembly version
        /// </summary>
        Version AssemblyVersion { get; }
    }

    /// <summary>
    /// Duck type class interface
    /// </summary>
    public interface IDuckTypeClass : IDuckType
    {
        /// <summary>
        /// Sets the instance object
        /// </summary>
        /// <param name="instance">Object instance value</param>
        void SetInstance(object instance);
    }
}
