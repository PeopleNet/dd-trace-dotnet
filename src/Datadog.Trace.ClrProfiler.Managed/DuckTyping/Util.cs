using System;
using System.Globalization;
using System.Reflection;

namespace Datadog.Trace.ClrProfiler.DuckTyping
{
    /// <summary>
    /// Utilities class
    /// </summary>
    public static class Util
    {
        internal static readonly MethodInfo GetTypeFromHandleMethodInfo = typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle));
        internal static readonly MethodInfo ConvertTypeMethodInfo = typeof(Util).GetMethod(nameof(Util.ConvertType));
        internal static readonly MethodInfo EnumToObjectMethodInfo = typeof(Enum).GetMethod(nameof(Enum.ToObject), new[] { typeof(Type), typeof(object) });

        /// <summary>
        /// Gets the root type
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Root type</returns>
        public static Type GetRootType(Type type)
        {
            while (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                type = Nullable.GetUnderlyingType(type);
            }

            return type;
        }

        /// <summary>
        /// Convert a value to an expected type
        /// </summary>
        /// <param name="value">Current value</param>
        /// <param name="conversionType">Expected type</param>
        /// <returns>Value with the new type</returns>
        public static object ConvertType(object value, Type conversionType)
            => value is IConvertible && value.GetType() != conversionType ? Convert.ChangeType(value, conversionType, CultureInfo.CurrentCulture) : value;
    }
}
