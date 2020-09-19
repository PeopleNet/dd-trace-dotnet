using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using Datadog.Trace.Logging;

namespace Datadog.Trace.ClrProfiler.DuckTyping
{
    /// <summary>
    /// Duck Type
    /// </summary>
    public static partial class DuckType
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly Vendors.Serilog.ILogger Log = DatadogLogging.GetLogger(typeof(DuckType));
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly object _locker = new object();
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly Dictionary<VTuple<Type, Type>, CreateTypeResult> DuckTypeCache = new Dictionary<VTuple<Type, Type>, CreateTypeResult>();
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly ConcurrentBag<DynamicMethod> DynamicMethods = new ConcurrentBag<DynamicMethod>();
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly MethodInfo DuckTypeCreateMethodInfo = typeof(DuckType).GetMethod(nameof(DuckType.Create), BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(Type), typeof(object) }, null);
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly PropertyInfo DuckTypeInstancePropertyInfo = typeof(IDuckType).GetProperty(nameof(IDuckType.Instance));
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly MethodInfo GetDuckTypeChainningValueMethodInfo = typeof(DuckType).GetMethod(nameof(GetDuckTypeChainningValue), BindingFlags.Static | BindingFlags.Public);

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static ModuleBuilder _moduleBuilder = null;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static Func<DynamicMethod, RuntimeMethodHandle> _dynamicGetMethodDescriptor;

        private static RuntimeMethodHandle GetRuntimeHandle(DynamicMethod dynamicMethod)
        {
            _dynamicGetMethodDescriptor ??= (Func<DynamicMethod, RuntimeMethodHandle>)typeof(DynamicMethod)
                .GetMethod("GetMethodDescriptor", BindingFlags.NonPublic | BindingFlags.Instance)
                .CreateDelegate(typeof(Func<DynamicMethod, RuntimeMethodHandle>));
            return _dynamicGetMethodDescriptor(dynamicMethod);
        }
     }
}
