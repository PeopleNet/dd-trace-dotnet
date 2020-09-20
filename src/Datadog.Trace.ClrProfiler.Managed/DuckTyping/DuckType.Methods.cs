using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Datadog.Trace.ClrProfiler.DuckTyping
{
    /// <summary>
    /// Duck Type
    /// </summary>
    public static partial class DuckType
    {
        private static List<MethodInfo> GetMethods(Type baseType)
        {
            List<MethodInfo> selectedMethods = new List<MethodInfo>(GetBaseMethods(baseType));
            Type[] implementedInterfaces = baseType.GetInterfaces();
            foreach (Type imInterface in implementedInterfaces)
            {
                if (imInterface == typeof(IDuckType))
                {
                    continue;
                }

                var newMethods = imInterface.GetMethods()
                    .Where(iMethod =>
                    {
                        if (iMethod.IsSpecialName)
                        {
                            return false;
                        }

                        string iMethodString = iMethod.ToString();
                        return selectedMethods.All(i => i.ToString() != iMethodString);
                    });
                selectedMethods.AddRange(newMethods);
            }

            return selectedMethods;
            static IEnumerable<MethodInfo> GetBaseMethods(Type baseType)
            {
                foreach (var method in baseType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    if (method.IsSpecialName || method.IsFinal || method.IsPrivate)
                    {
                        continue;
                    }

                    if (baseType.IsInterface || method.IsAbstract || method.IsVirtual)
                    {
                        yield return method;
                    }
                }
            }
        }

        private static void CreateMethods(TypeBuilder proxyTypeBuilder, Type proxyType, Type targetType, FieldInfo instanceField)
        {
            List<MethodInfo> proxyMethodsDefinitions = GetMethods(proxyType);
            foreach (MethodInfo proxyMethodDefinition in proxyMethodsDefinitions)
            {
                // Extract the method parameters types
                ParameterInfo[] proxyMethodDefinitionParameters = proxyMethodDefinition.GetParameters();
                Type[] proxyMethodDefinitionParametersTypes = proxyMethodDefinitionParameters.Select(p => p.ParameterType).ToArray();

                // We select the target method to call
                MethodInfo targetMethod = SelectTargetMethod(targetType, proxyMethodDefinition, proxyMethodDefinitionParameters, proxyMethodDefinitionParametersTypes);
                if (targetMethod is null && proxyMethodDefinition.IsVirtual)
                {
                    throw new DuckTypeTargetMethodNotFoundException(proxyMethodDefinition);
                }

                // Make sure we have the right methods attributes, for proxy methods declared in abstract and virtual classes
                // a new slot on the vtable is not required, for interfaces is required.
                MethodAttributes proxyMethodAttributes = MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.Final | MethodAttributes.HideBySig;
                if (!proxyMethodDefinition.IsAbstract && !proxyMethodDefinition.IsVirtual)
                {
                    proxyMethodAttributes |= MethodAttributes.NewSlot;
                }

                // Gets the proxy method definition generic arguments
                Type[] proxyMethodDefinitionGenericArguments = proxyMethodDefinition.GetGenericArguments();
                string[] proxyMethodDefinitionGenericArgumentsNames = proxyMethodDefinitionGenericArguments.Select((t, i) => "T" + (i + 1)).ToArray();

                // Create the proxy method implementation
                ParameterBuilder[] proxyMethodParametersBuilders = new ParameterBuilder[proxyMethodDefinitionParameters.Length];
                MethodBuilder proxyMethod = proxyTypeBuilder.DefineMethod(proxyMethodDefinition.Name, proxyMethodAttributes, proxyMethodDefinition.ReturnType, proxyMethodDefinitionParametersTypes);
                if (proxyMethodDefinitionGenericArgumentsNames.Length > 0)
                {
                    proxyMethod.DefineGenericParameters(proxyMethodDefinitionGenericArgumentsNames);
                }

                // Create the proxy method implementation parameters
                for (var j = 0; j < proxyMethodDefinitionParameters.Length; j++)
                {
                    ParameterInfo pmDefParameter = proxyMethodDefinitionParameters[j];
                    ParameterBuilder pmImpParameter = proxyMethod.DefineParameter(j, pmDefParameter.Attributes, pmDefParameter.Name);
                    if (pmDefParameter.HasDefaultValue)
                    {
                        pmImpParameter.SetConstant(pmDefParameter.RawDefaultValue);
                    }

                    proxyMethodParametersBuilders[j] = pmImpParameter;
                }

                var il = proxyMethod.GetILGenerator();
                var publicInstance = targetType.IsPublic || targetType.IsNestedPublic;
                var innerDuck = false;
                var iMethodReturnType = proxyMethodDefinition.ReturnType;
                if (iMethodReturnType.IsGenericType)
                {
                    iMethodReturnType = iMethodReturnType.GetGenericTypeDefinition();
                }

                if (proxyMethodDefinition.ReturnType != targetMethod.ReturnType &&
                    !proxyMethodDefinition.ReturnType.IsValueType && !proxyMethodDefinition.ReturnType.IsAssignableFrom(targetMethod.ReturnType) &&
                    !proxyMethodDefinition.ReturnType.IsGenericParameter && !targetMethod.ReturnType.IsGenericParameter)
                {
                    il.Emit(OpCodes.Ldtoken, proxyMethodDefinition.ReturnType);
                    il.EmitCall(OpCodes.Call, Util.GetTypeFromHandleMethodInfo, null);
                    innerDuck = true;
                }

                // Create generic method call
                if (proxyMethodDefinitionGenericArguments.Length > 0)
                {
                    targetMethod = targetMethod.MakeGenericMethod(proxyMethodDefinitionGenericArguments);
                }

                if (publicInstance)
                {
                    // Load instance
                    if (!targetMethod.IsStatic)
                    {
                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Ldfld, instanceField);
                    }

                    // Load arguments
                    var parameters = targetMethod.GetParameters();
                    var minParametersLength = Math.Min(parameters.Length, proxyMethodDefinitionParameters.Length);
                    for (var i = 0; i < minParametersLength; i++)
                    {
                        // Load value
                        ILHelpers.WriteLoadArgument(i, il, proxyMethodDefinition.IsStatic);
                        var iPType = Util.GetRootType(proxyMethodDefinitionParameters[i].ParameterType);
                        var pType = Util.GetRootType(parameters[i].ParameterType);
                        ILHelpers.TypeConversion(il, iPType, pType);
                    }

                    // Call method
                    if (targetMethod.IsPublic)
                    {
                        il.EmitCall(targetMethod.IsStatic ? OpCodes.Call : OpCodes.Callvirt, targetMethod, null);
                    }
                    else
                    {
                        il.Emit(OpCodes.Ldc_I8, (long)targetMethod.MethodHandle.GetFunctionPointer());
                        il.Emit(OpCodes.Conv_I);
                        il.EmitCalli(
                            OpCodes.Calli,
                            targetMethod.CallingConvention,
                            targetMethod.ReturnType,
                            targetMethod.GetParameters().Select(p => p.ParameterType).ToArray(),
                            null);
                    }

                    // Covert return value
                    if (targetMethod.ReturnType != typeof(void))
                    {
                        if (innerDuck)
                        {
                            ILHelpers.TypeConversion(il, targetMethod.ReturnType, typeof(object));
                            il.EmitCall(OpCodes.Call, DuckTypeCreateMethodInfo, null);
                        }
                        else if (targetMethod.ReturnType != proxyMethodDefinition.ReturnType)
                        {
                            ILHelpers.TypeConversion(il, targetMethod.ReturnType, proxyMethodDefinition.ReturnType);
                        }
                    }
                }
                else
                {
                    if (!targetMethod.IsStatic)
                    {
                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Ldfld, instanceField);
                    }
                    else
                    {
                        il.Emit(OpCodes.Ldnull);
                    }

                    // Load arguments
                    var parameters = targetMethod.GetParameters();
                    var minParametersLength = Math.Min(parameters.Length, proxyMethodDefinitionParameters.Length);
                    ILHelpers.WriteIlIntValue(il, minParametersLength);
                    il.Emit(OpCodes.Newarr, typeof(object));
                    for (var i = 0; i < minParametersLength; i++)
                    {
                        // Load value
                        il.Emit(OpCodes.Dup);
                        ILHelpers.WriteIlIntValue(il, i);
                        ILHelpers.WriteLoadArgument(i, il, proxyMethodDefinition.IsStatic);
                        var iPType = Util.GetRootType(proxyMethodDefinitionParameters[i].ParameterType);
                        ILHelpers.TypeConversion(il, iPType, typeof(object));
                        il.Emit(OpCodes.Stelem_Ref);
                    }

                    var dynParameters = new[] { typeof(object), typeof(object[]) };
                    var dynMethod = new DynamicMethod("callDyn_" + targetMethod.Name, typeof(object), dynParameters, typeof(DuckType).Module, true);
                    CreateMethodAccessor(dynMethod.GetILGenerator(), targetMethod, false);
                    var handle = GetRuntimeHandle(dynMethod);

                    il.Emit(OpCodes.Ldc_I8, (long)handle.GetFunctionPointer());
                    il.Emit(OpCodes.Conv_I);
                    il.EmitCalli(OpCodes.Calli, dynMethod.CallingConvention, dynMethod.ReturnType, dynParameters, null);
                    DynamicMethods.Add(dynMethod);

                    // Convert return value
                    if (targetMethod.ReturnType != typeof(void))
                    {
                        if (innerDuck)
                        {
                            il.EmitCall(OpCodes.Call, DuckTypeCreateMethodInfo, null);
                        }
                        else if (proxyMethodDefinition.ReturnType != typeof(object))
                        {
                            ILHelpers.TypeConversion(il, typeof(object), proxyMethodDefinition.ReturnType);
                        }
                    }
                    else
                    {
                        il.Emit(OpCodes.Pop);
                    }
                }

                il.Emit(OpCodes.Ret);
            }
        }

        private static MethodInfo SelectTargetMethod(Type targetType, MethodInfo proxyMethod, ParameterInfo[] parameters, Type[] parametersTypes)
        {
            var asmVersion = targetType.Assembly.GetName().Version;
            var duckAttrs = proxyMethod.GetCustomAttributes<DuckAttribute>(true).ToList();
            if (duckAttrs.Count == 0)
            {
                duckAttrs.Add(new DuckAttribute());
            }

            var iMethodString = proxyMethod.ToString();
            MethodAttributesSelector[] allMethods = null!;
            foreach (var duckAttr in duckAttrs)
            {
                duckAttr.Name ??= proxyMethod.Name;

                // We select the method to call
                var method = targetType.GetMethod(duckAttr.Name, duckAttr.BindingFlags, null, parametersTypes, null);

                if (!(method is null))
                {
                    var attrs = method.GetCustomAttributes<DuckAttribute>().ToList();
                    if (attrs.Count == 0)
                    {
                        return method;
                    }

                    foreach (var attribute in attrs)
                    {
                        if (attribute.Name == iMethodString)
                        {
                            return method;
                        }
                    }
                }

                if (allMethods is null)
                {
                    var methods = targetType.GetMethods(duckAttr.BindingFlags);
                    allMethods = new MethodAttributesSelector[methods.Length];
                    for (var i = 0; i < allMethods.Length; i++)
                    {
                        allMethods[i] = new MethodAttributesSelector(
                            methods[i],
                            new List<DuckAttribute>(methods[i].GetCustomAttributes<DuckAttribute>(true)));
                    }
                }

                var remaining = allMethods.Where(ma =>
                {
                    if (ma.Attributes.Count == 0)
                    {
                        if (ma.Method.Name != duckAttr.Name)
                        {
                            return false;
                        }

                        // Trying to select the ones with the same parameters count
                        var mParams = ma.Method.GetParameters();
                        if (mParams.Length == parameters.Length)
                        {
                            return true;
                        }

                        var min = Math.Min(mParams.Length, parameters.Length);
                        var max = Math.Max(mParams.Length, parameters.Length);
                        for (var i = min; i < max; i++)
                        {
                            if (mParams.Length > i && !mParams[i].HasDefaultValue)
                            {
                                return false;
                            }

                            if (parameters.Length > i && !parameters[i].HasDefaultValue)
                            {
                                return false;
                            }
                        }
                        return true;
                    }

                    // Trying to select the one with the same name (used by reverse proxy)
                    foreach (var attribute in ma.Attributes)
                    {
                        if (attribute.Name == iMethodString)
                        {
                            return true;
                        }
                    }
                    return false;
                }).ToList();

                if (remaining.Count == 0)
                {
                    continue;
                }

                if (remaining.Count == 1)
                {
                    return remaining[0].Method;
                }

                var remainWithAttribute = remaining.FirstOrDefault(r => r.Attributes.Count > 0);
                if (!(remainWithAttribute.Method is null))
                {
                    return remainWithAttribute.Method;
                }

                // Trying to select the ones with the same return type
                var sameReturnType = remaining.Where(ma => ma.Method.ReturnType == proxyMethod.ReturnType).ToList();
                if (sameReturnType.Count == 1)
                {
                    return sameReturnType[0].Method;
                }

                if (sameReturnType.Count > 1)
                {
                    remaining = sameReturnType;
                }

                if (proxyMethod.ReturnType.IsInterface && proxyMethod.ReturnType.GetInterface(proxyMethod.ReturnType.FullName) == null)
                {
                    var duckReturnType = remaining.Where(ma => !ma.Method.ReturnType.IsValueType).ToList();
                    if (duckReturnType.Count == 1)
                    {
                        return duckReturnType[0].Method;
                    }

                    if (duckReturnType.Count > 1)
                    {
                        remaining = duckReturnType;
                    }
                }

                // Trying to select the one with the same parameters types
                var sameParameters = remaining.Where(m =>
                {
                    var mParams = m.Method.GetParameters();
                    var min = Math.Min(mParams.Length, parameters.Length);
                    for (var i = 0; i < min; i++)
                    {
                        var expectedType = mParams[i].ParameterType;
                        var actualType = parameters[i].ParameterType;

                        if (expectedType == actualType)
                        {
                            continue;
                        }

                        if (expectedType.IsAssignableFrom(actualType))
                        {
                            continue;
                        }

                        if (!expectedType.IsValueType && actualType == typeof(object))
                        {
                            continue;
                        }

                        if (expectedType.IsValueType && actualType.IsValueType)
                        {
                            continue;
                        }

                        return false;
                    }
                    return true;
                }).ToList();

                if (sameParameters.Count == 1)
                {
                    return sameParameters[0].Method;
                }

                return remaining[0].Method;
            }

            return null;
        }

        private static void CreateMethodAccessor(ILGenerator il, MethodInfo method, bool strict)
        {
            // Prepare instance
            if (!method.IsStatic)
            {
                il.Emit(OpCodes.Ldarg_0);
                if (method.DeclaringType!.IsValueType)
                {
                    il.Emit(OpCodes.Unbox_Any, method.DeclaringType);
                    il.Emit(OpCodes.Stloc_0);
                    il.Emit(OpCodes.Ldloca_S, 0);
                }
                else if (method.DeclaringType != typeof(object))
                {
                    il.Emit(OpCodes.Castclass, method.DeclaringType);
                }
            }

            // Prepare arguments
            var parameters = method.GetParameters();
            for (var i = 0; i < parameters.Length; i++)
            {
                var pType = parameters[i].ParameterType;
                var rType = Util.GetRootType(pType);
                var callEnum = false;
                if (rType.IsEnum)
                {
                    il.Emit(OpCodes.Ldtoken, rType);
                    il.EmitCall(OpCodes.Call, Util.GetTypeFromHandleMethodInfo, null);
                    callEnum = true;
                }

                il.Emit(OpCodes.Ldarg_1);
                ILHelpers.WriteIlIntValue(il, i);
                il.Emit(OpCodes.Ldelem_Ref);

                if (callEnum)
                {
                    il.EmitCall(OpCodes.Call, Util.EnumToObjectMethodInfo, null);
                }
                else if (!strict && pType != typeof(object))
                {
                    il.Emit(OpCodes.Ldtoken, rType);
                    il.EmitCall(OpCodes.Call, Util.GetTypeFromHandleMethodInfo, null);
                    il.EmitCall(OpCodes.Call, Util.ConvertTypeMethodInfo, null);
                }

                if (pType.IsValueType)
                {
                    il.Emit(OpCodes.Unbox_Any, pType);
                }
                else if (pType != typeof(object))
                {
                    il.Emit(OpCodes.Castclass, pType);
                }
            }

            // Call method
            il.EmitCall(method.IsStatic ? OpCodes.Call : OpCodes.Callvirt, method, null);

            // Prepare return
            if (method.ReturnType == typeof(void))
            {
                il.Emit(OpCodes.Ldnull);
            }
            else if (method.ReturnType.IsValueType)
            {
                il.Emit(OpCodes.Box, method.ReturnType);
            }

            il.Emit(OpCodes.Ret);
        }

        private readonly struct MethodAttributesSelector
        {
            public readonly MethodInfo Method;
            public readonly List<DuckAttribute> Attributes;

            public MethodAttributesSelector(MethodInfo methodInfo, List<DuckAttribute> attributes)
            {
                Method = methodInfo;
                Attributes = attributes;
            }
        }
    }
}
