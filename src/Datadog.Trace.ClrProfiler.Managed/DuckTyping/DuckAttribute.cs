using System;
using System.Reflection;

namespace Datadog.Trace.ClrProfiler.DuckTyping
{
    /// <summary>
    /// Duck kind
    /// </summary>
    public enum DuckKind
    {
        /// <summary>
        /// Property
        /// </summary>
        Property,

        /// <summary>
        /// Field
        /// </summary>
        Field
    }

    /// <summary>
    /// Duck attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = true)]
    public class DuckAttribute : Attribute
    {
        /// <summary>
        /// All flags for static, non static, public and non public members
        /// </summary>
        public const BindingFlags AllFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        private string _upToVersion;

        /// <summary>
        /// Gets or sets property Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets binding flags
        /// </summary>
        public BindingFlags BindingFlags { get; set; } = AllFlags;

        /// <summary>
        /// Gets or sets duck kind
        /// </summary>
        public DuckKind Kind { get; set; } = DuckKind.Property;

        /// <summary>
        /// Gets or sets up to assembly version
        /// </summary>
        public string UpToVersion
        {
            get => _upToVersion;
            set
            {
                Version = string.IsNullOrWhiteSpace(value) ? null : new Version(value);
                _upToVersion = value;
            }
        }

        /// <summary>
        /// Gets internal up to assembly version
        /// </summary>
        internal Version Version { get; private set; }
    }
}
