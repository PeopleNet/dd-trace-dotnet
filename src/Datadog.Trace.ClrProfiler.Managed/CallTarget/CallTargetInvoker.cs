using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Datadog.Trace.ClrProfiler.DuckTyping;
using Datadog.Trace.ClrProfiler.Integrations.Testing;
using Datadog.Trace.Logging;

namespace Datadog.Trace.ClrProfiler.CallTarget
{
    internal delegate CallTargetState MethodBeginDelegate(object instance, object[] arguments);

    internal delegate object MethodEndDelegate(object returnValue, Exception exception, CallTargetState state);

    /// <summary>
    /// Call target integration helper
    /// </summary>
    public static class CallTargetInvoker
    {
        private static readonly Vendors.Serilog.ILogger Log = DatadogLogging.GetLogger(typeof(CallTargetInvoker));
        private static readonly MethodInfo ConvertTypeMethodInfo = typeof(CallTargetInvoker).GetMethod(nameof(CallTargetInvoker.ConvertType), BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly MethodInfo UnWrapReturnValueMethodInfo = typeof(CallTargetInvoker).GetMethod(nameof(CallTargetInvoker.UnWrapReturnValue), BindingFlags.NonPublic | BindingFlags.Static);

        /// <summary>
        /// Call target static begin method helper
        /// </summary>
        /// <typeparam name="TWrapper">Type of the wrapper</typeparam>
        /// <typeparam name="TInstance">Type of the instance</typeparam>
        /// <param name="instance">Object instance</param>
        /// <param name="arguments">Arguments</param>
        /// <returns>CallTargetBeginReturn instance</returns>
        public static CallTargetState BeginMethod<TWrapper, TInstance>(TInstance instance, object[] arguments)
        {
            return CallTargetIntegration<TWrapper, TInstance>.BeginMethod(instance, arguments);
        }

        /// <summary>
        /// Call target static end method helper
        /// </summary>
        /// <typeparam name="TWrapper">Type of the wrapper</typeparam>
        /// <typeparam name="TInstance">Type of the instance</typeparam>
        /// <param name="returnValue">Original method return value</param>
        /// <param name="exception">Original method exception</param>
        /// <param name="state">State from the BeginMethod</param>
        /// <returns>Return value</returns>
        public static object EndMethod<TWrapper, TInstance>(object returnValue, Exception exception, CallTargetState state)
        {
            return CallTargetIntegration<TWrapper, TInstance>.EndMethod(returnValue, exception, state);
        }

        /// <summary>
        /// Logs the exception to the CallTarget Logger
        /// </summary>
        /// <param name="ex">Exception instance</param>
        public static void LogException(Exception ex)
        {
            if (ex != null)
            {
                Log.SafeLogError(ex, ex.Message);
            }
        }

        private static MethodBeginDelegate CreateMethodBeginDelegate<TInstance>(Type wrapperType, string methodName)
        {
            try
            {
                Log.Information($"Creating MethodBegin Delegate: {methodName} in {wrapperType.FullName}");
                MethodInfo onMethodBeginMethodInfo = wrapperType.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                if (onMethodBeginMethodInfo is null)
                {
                    Log.Warning($"Couldn't find the method: {methodName} in type: {wrapperType.FullName}");
                    return null;
                }

                if (onMethodBeginMethodInfo.ReturnType != typeof(CallTargetState))
                {
                    Log.Error($"The return type of the method: {methodName} in type: {wrapperType.FullName} is not {nameof(CallTargetState)}");
                    return null;
                }

                Type[] genericArgumentsTypes = onMethodBeginMethodInfo.GetGenericArguments();

                Type[] genericInstanceConstraints = genericArgumentsTypes[0].GetGenericParameterConstraints();
                bool hasInstanceContraint = genericInstanceConstraints.Length > 0;
                List<Type> callGenericTypes = new List<Type>();

                if (hasInstanceContraint)
                {
                    var result = DuckType.GetOrCreateProxyType(genericInstanceConstraints[0], typeof(TInstance));
                    result.ExceptionInfo?.Throw();
                    callGenericTypes.Add(result.ProxyType);
                }
                else
                {
                    callGenericTypes.Add(typeof(TInstance));
                }

                DynamicMethod callMethod = new DynamicMethod(
                    $"{onMethodBeginMethodInfo.DeclaringType.Name}.{onMethodBeginMethodInfo.Name}",
                    typeof(CallTargetState),
                    new Type[] { typeof(object), typeof(object[]) },
                    onMethodBeginMethodInfo.Module);
                ILGenerator ilWriter = callMethod.GetILGenerator();

                ParameterInfo[] parameters = onMethodBeginMethodInfo.GetParameters();

                ilWriter.Emit(OpCodes.Ldarg_0);
                if (hasInstanceContraint)
                {
                    ilWriter.Emit(OpCodes.Ldtoken, genericInstanceConstraints[0]);
                    ilWriter.EmitCall(OpCodes.Call, DuckTyping.Util.GetTypeFromHandleMethodInfo, null);
                    ilWriter.EmitCall(OpCodes.Call, ConvertTypeMethodInfo, null);
                }
                else
                {
                    if (typeof(TInstance).IsValueType)
                    {
                        ilWriter.Emit(OpCodes.Unbox_Any, typeof(TInstance));
                    }
                    else if (typeof(TInstance) != typeof(object))
                    {
                        ilWriter.Emit(OpCodes.Castclass, typeof(TInstance));
                    }
                }

                // Load arguments
                for (var i = 1; i < parameters.Length; i++)
                {
                    Type pType = parameters[i].ParameterType;

                    if (pType.IsGenericParameter)
                    {
                        pType = genericArgumentsTypes[pType.GenericParameterPosition];
                        Type[] genericConstraints = pType.GetGenericParameterConstraints();
                        if (genericConstraints.Length > 0)
                        {
                            pType = genericConstraints[0];
                            callGenericTypes.Add(pType);
                        }
                        else
                        {
                            ilWriter.Emit(OpCodes.Ldarg_1);
                            ILHelpers.WriteIlIntValue(ilWriter, i - 1);
                            ilWriter.Emit(OpCodes.Ldelem_Ref);
                            callGenericTypes.Add(typeof(object));
                            continue;
                        }
                    }

                    Type rType = DuckTyping.Util.GetRootType(pType);
                    bool callEnum = false;
                    if (rType.IsEnum)
                    {
                        ilWriter.Emit(OpCodes.Ldtoken, rType);
                        ilWriter.EmitCall(OpCodes.Call, DuckTyping.Util.GetTypeFromHandleMethodInfo, null);
                        callEnum = true;
                    }

                    ilWriter.Emit(OpCodes.Ldarg_1);
                    ILHelpers.WriteIlIntValue(ilWriter, i - 1);
                    ilWriter.Emit(OpCodes.Ldelem_Ref);

                    if (callEnum)
                    {
                        ilWriter.EmitCall(OpCodes.Call, DuckTyping.Util.EnumToObjectMethodInfo, null);
                    }
                    else
                    {
                        ilWriter.Emit(OpCodes.Ldtoken, rType);
                        ilWriter.EmitCall(OpCodes.Call, DuckTyping.Util.GetTypeFromHandleMethodInfo, null);
                        ilWriter.EmitCall(OpCodes.Call, ConvertTypeMethodInfo, null);
                    }

                    if (pType.IsValueType)
                    {
                        ilWriter.Emit(OpCodes.Unbox_Any, pType);
                    }
                    else if (pType != typeof(object))
                    {
                        ilWriter.Emit(OpCodes.Castclass, pType);
                    }
                }

                // Call method
                Log.Information("Generic Types: " + string.Join(", ", callGenericTypes.Select(t => t.FullName)));
                onMethodBeginMethodInfo = onMethodBeginMethodInfo.MakeGenericMethod(callGenericTypes.ToArray());
                ilWriter.EmitCall(OpCodes.Call, onMethodBeginMethodInfo, null);
                ilWriter.Emit(OpCodes.Ret);

                return (MethodBeginDelegate)callMethod.CreateDelegate(typeof(MethodBeginDelegate));
            }
            catch (Exception ex)
            {
                Log.SafeLogError(ex, $"Error while creating the MethodBegin delegate for {methodName}");
            }

            return null;
        }

        private static MethodEndDelegate CreateMethodEndDelegate(Type wrapperType, string methodName)
        {
            try
            {
                Log.Information($"Creating MethodEnd Delegate: {methodName} in {wrapperType.FullName}");
                MethodInfo onMethodEndMethodInfo = wrapperType.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                if (onMethodEndMethodInfo is null)
                {
                    Log.Warning($"Couldn't find the method: {methodName} in type: {wrapperType.FullName}");
                    return null;
                }

                ParameterInfo[] parameters = onMethodEndMethodInfo.GetParameters();
                if (parameters.Length != 3)
                {
                    Log.Error($"The method: {methodName} in type: {wrapperType.FullName} should have exactly 3 parameters.");
                    return null;
                }

                DynamicMethod callMethod = new DynamicMethod(
                    $"{onMethodEndMethodInfo.DeclaringType.Name}.{onMethodEndMethodInfo.Name}",
                    typeof(object),
                    new Type[] { typeof(object), typeof(Exception), typeof(CallTargetState) },
                    onMethodEndMethodInfo.Module);
                ILGenerator ilWriter = callMethod.GetILGenerator();

                // Load the return value
                Type pType = parameters[0].ParameterType;
                Type rType = DuckTyping.Util.GetRootType(pType);
                bool callEnum = false;
                if (rType.IsEnum)
                {
                    ilWriter.Emit(OpCodes.Ldtoken, rType);
                    ilWriter.EmitCall(OpCodes.Call, DuckTyping.Util.GetTypeFromHandleMethodInfo, null);
                    callEnum = true;
                }

                ILHelpers.WriteLoadArgument(0, ilWriter, true);
                if (callEnum)
                {
                    ilWriter.EmitCall(OpCodes.Call, DuckTyping.Util.EnumToObjectMethodInfo, null);
                }
                else
                {
                    ilWriter.Emit(OpCodes.Ldtoken, rType);
                    ilWriter.EmitCall(OpCodes.Call, DuckTyping.Util.GetTypeFromHandleMethodInfo, null);
                    ilWriter.EmitCall(OpCodes.Call, ConvertTypeMethodInfo, null);
                }

                if (pType.IsValueType)
                {
                    ilWriter.Emit(OpCodes.Unbox_Any, pType);
                }
                else if (pType != typeof(object))
                {
                    ilWriter.Emit(OpCodes.Castclass, pType);
                }

                // Load exception
                ILHelpers.WriteLoadArgument(1, ilWriter, true);

                // Load state
                ILHelpers.WriteLoadArgument(2, ilWriter, true);

                // Call method
                ilWriter.EmitCall(OpCodes.Call, onMethodEndMethodInfo, null);

                // Unwrap in case the return value is a duck type
                ilWriter.EmitCall(OpCodes.Call, UnWrapReturnValueMethodInfo, null);
                ilWriter.Emit(OpCodes.Ret);

                return (MethodEndDelegate)callMethod.CreateDelegate(typeof(MethodEndDelegate));
            }
            catch (Exception ex)
            {
                Log.SafeLogError(ex, $"Error while creating the MethodEnd delegate for {methodName}");
            }

            return null;
        }

        private static object ConvertType(object value, Type conversionType)
        {
            if (value is null || conversionType == typeof(object))
            {
                return value;
            }

            Type valueType = value.GetType();
            if (valueType == conversionType || conversionType.IsAssignableFrom(valueType))
            {
                return value;
            }

            // if (value is IConvertible)
            // {
            //     return Convert.ChangeType(value, conversionType, CultureInfo.CurrentCulture);
            // }

            // Finally we try to duck type
            return DuckType.Create(conversionType, value);
        }

        private static object UnWrapReturnValue(object returnValue)
        {
            if (returnValue is IDuckType dType)
            {
                return dType.Instance;
            }

            return returnValue;
        }

        private static class CallTargetIntegration<TWrapper, TInstance>
        {
            private static MethodBeginDelegate _onMethodBeginDelegate;
            private static MethodEndDelegate _onMethodEndDelegate;
            private static MethodEndDelegate _onAsyncMethodEndDelegate;

            static CallTargetIntegration()
            {
                Type wrapperType = typeof(TWrapper);
                _onMethodBeginDelegate = CallTargetInvoker.CreateMethodBeginDelegate<TInstance>(wrapperType, "OnMethodBegin");
                _onMethodEndDelegate = CallTargetInvoker.CreateMethodEndDelegate(wrapperType, "OnMethodEnd");
                _onAsyncMethodEndDelegate = CallTargetInvoker.CreateMethodEndDelegate(wrapperType, "OnAsyncMethodEnd");
            }

            public static CallTargetState BeginMethod(TInstance instance, object[] arguments)
            {
                if (_onMethodBeginDelegate != null)
                {
                    return _onMethodBeginDelegate(instance, arguments);
                }

                return new CallTargetState(null);
            }

            public static object EndMethod(object returnValue, Exception exception, CallTargetState state)
            {
                if (_onMethodEndDelegate != null)
                {
                    returnValue = _onMethodEndDelegate(returnValue, exception, state);
                }

                if (_onAsyncMethodEndDelegate != null)
                {
                    returnValue = AsyncTool.AddContinuation(
                        returnValue,
                        exception,
                        new VTuple<MethodEndDelegate, CallTargetState>(_onAsyncMethodEndDelegate, state),
                        (rValue, ex, tuple) =>
                        {
                            try
                            {
                                return tuple.Item1(rValue, ex, tuple.Item2);
                            }
                            catch (Exception cEx)
                            {
                                LogException(cEx);
                            }

                            return rValue;
                        });
                }

                return returnValue;
            }
        }
    }
}
