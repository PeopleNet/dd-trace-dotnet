using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Datadog.Trace.ClrProfiler.CallTarget.DuckTyping
{
    /// <summary>
    /// Duck Type
    /// </summary>
    public static partial class DuckType
    {
        private static MethodBuilder GetPropertyGetMethod(TypeBuilder proxyTypeBuilder, Type targetType, PropertyInfo proxyProperty, PropertyInfo targetProperty, FieldInfo instanceField)
        {
            Type[] proxyParameterTypes = GetPropertyGetParametersTypes(proxyProperty, true).ToArray();
            Type[] targetParametersTypes = GetPropertyGetParametersTypes(targetProperty, true).ToArray();
            if (proxyParameterTypes.Length != targetParametersTypes.Length)
            {
                throw new DuckTypePropertyArgumentsLengthException(proxyProperty);
            }

            MethodBuilder proxyMethod = proxyTypeBuilder.DefineMethod(
                "get_" + proxyProperty.Name,
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.Virtual,
                proxyProperty.PropertyType,
                proxyParameterTypes);

            ILGenerator il = proxyMethod.GetILGenerator();
            MethodInfo targetMethod = targetProperty.GetMethod;
            bool publicInstance = targetType.IsPublic || targetType.IsNestedPublic;
            Type returnType = targetProperty.PropertyType;
            bool duckChaining = false;

            // Check if the type can be converted of if we need to enable duck chaining
            if (proxyProperty.PropertyType != targetProperty.PropertyType && !proxyProperty.PropertyType.IsValueType && !proxyProperty.PropertyType.IsAssignableFrom(targetProperty.PropertyType))
            {
                // Create and load the duck type field reference to the stack
                if (targetMethod.IsStatic)
                {
                    FieldInfo innerDuckField = DynamicFields.GetOrAdd(new VTuple<string, TypeBuilder>($"_duckStatic_{proxyProperty.Name}_{proxyParameterTypes.Length}", proxyTypeBuilder), tuple =>
                        tuple.Item2.DefineField(tuple.Item1, typeof(IDuckType), FieldAttributes.Private | FieldAttributes.Static));
                    il.Emit(OpCodes.Ldsflda, innerDuckField);
                }
                else
                {
                    FieldInfo innerDuckField = DynamicFields.GetOrAdd(new VTuple<string, TypeBuilder>($"_duck_{proxyProperty.Name}_{proxyParameterTypes.Length}", proxyTypeBuilder), tuple =>
                        tuple.Item2.DefineField(tuple.Item1, typeof(IDuckType), FieldAttributes.Private));
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldflda, innerDuckField);
                }

                // Load the property type to the stack
                il.Emit(OpCodes.Ldtoken, proxyProperty.PropertyType);
                il.EmitCall(OpCodes.Call, Util.GetTypeFromHandleMethodInfo, null);
                duckChaining = true;
            }

            // Load the instance if needed
            if (!targetMethod.IsStatic)
            {
                ILHelpers.LoadInstance(il, instanceField, targetType);
            }

            // Load the indexer keys to the stack
            for (int pIndex = 0; pIndex < proxyParameterTypes.Length; pIndex++)
            {
                Type proxyParamType = Util.GetRootType(proxyParameterTypes[pIndex]);
                Type targetParamType = Util.GetRootType(targetParametersTypes[pIndex]);

                // Check if the type can be converted of if we need to enable duck chaining
                if (proxyParamType != targetParamType && !proxyParamType.IsValueType && !proxyParamType.IsAssignableFrom(targetProperty.PropertyType))
                {
                    // Create and load the duck type field reference to the stack
                    if (targetMethod.IsStatic)
                    {
                        FieldInfo innerField = DynamicFields.GetOrAdd(new VTuple<string, TypeBuilder>($"_duckStatic_{proxyProperty.Name}_{pIndex}", proxyTypeBuilder), tuple =>
                            tuple.Item2.DefineField(tuple.Item1, typeof(IDuckType), FieldAttributes.Private | FieldAttributes.Static));
                        il.Emit(OpCodes.Ldsflda, innerField);
                    }
                    else
                    {
                        FieldInfo innerField = DynamicFields.GetOrAdd(new VTuple<string, TypeBuilder>($"_duck_{proxyProperty.Name}_{pIndex}", proxyTypeBuilder), tuple =>
                            tuple.Item2.DefineField(tuple.Item1, typeof(IDuckType), FieldAttributes.Private));
                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Ldflda, innerField);
                    }

                    // Load the property type to the stack
                    il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Castclass, typeof(IDuckType));
                    il.EmitCall(OpCodes.Call, SetInnerDuckTypeMethodInfo, null);
                    targetParamType = typeof(object);
                }
                else
                {
                    ILHelpers.WriteLoadArgument(pIndex, il, false);
                    targetParamType = targetParamType.IsPublic || targetParamType.IsNestedPublic ? targetParamType : typeof(object);
                    ILHelpers.TypeConversion(il, proxyParamType, targetParamType);
                }

                targetParametersTypes[pIndex] = targetParamType;
            }

            // Call the set method
            if (publicInstance)
            {
                // If the instance is public we can emit directly without any dynamic method

                // Method call
                if (targetMethod.IsPublic)
                {
                    // We can emit a normal call if we have a public instance with a public property method.
                    il.EmitCall(targetMethod.IsStatic ? OpCodes.Call : OpCodes.Callvirt, targetMethod, null);
                }
                else
                {
                    // In case we have a public instance and a non public property method we can use [Calli] with the function pointer
                    il.Emit(OpCodes.Ldc_I8, (long)targetMethod.MethodHandle.GetFunctionPointer());
                    il.Emit(OpCodes.Conv_I);
                    il.EmitCalli(
                        OpCodes.Calli,
                        targetMethod.CallingConvention,
                        targetMethod.ReturnType,
                        targetMethod.GetParameters().Select(p => p.ParameterType).ToArray(),
                        null);
                }
            }
            else
            {
                // If the instance is not public we need to create a Dynamic method to overpass the visibility checks
                // we can't access non public types so we have to cast to object type (in the instance object and the return type).

                string dynMethodName = $"_getNonPublicProperty+{targetProperty.DeclaringType.Name}.{targetProperty.Name}";
                returnType = targetProperty.PropertyType.IsPublic || targetProperty.PropertyType.IsNestedPublic ? targetProperty.PropertyType : typeof(object);

                // We create the dynamic method
                Type[] targetParameters = GetPropertyGetParametersTypes(targetProperty, true, !targetMethod.IsStatic).ToArray();
                Type[] dynParameters = targetMethod.IsStatic ? targetParametersTypes : (new[] { typeof(object) }).Concat(targetParametersTypes).ToArray();
                DynamicMethod dynMethod = new DynamicMethod(dynMethodName, returnType, dynParameters, typeof(DuckType).Module, true);

                // We store the dynamic method in a bag to avoid getting collected by the GC.
                DynamicMethods.Add(dynMethod);

                // Emit the dynamic method body
                ILGenerator dynIL = dynMethod.GetILGenerator();

                if (!targetMethod.IsStatic)
                {
                    ILHelpers.LoadInstanceArgument(dynIL, typeof(object), targetProperty.DeclaringType);
                }

                for (int idx = targetMethod.IsStatic ? 0 : 1; idx < dynParameters.Length; idx++)
                {
                    ILHelpers.WriteLoadArgument(idx, dynIL, true);
                    ILHelpers.TypeConversion(dynIL, dynParameters[idx], targetParameters[idx]);
                }

                dynIL.EmitCall(targetMethod.IsStatic ? OpCodes.Call : OpCodes.Callvirt, targetMethod, null);
                ILHelpers.TypeConversion(dynIL, targetProperty.PropertyType, returnType);
                dynIL.Emit(OpCodes.Ret);

                // Emit the Call to the dynamic method pointer [Calli]
                il.Emit(OpCodes.Ldc_I8, (long)GetRuntimeHandle(dynMethod).GetFunctionPointer());
                il.Emit(OpCodes.Conv_I);
                il.EmitCalli(OpCodes.Calli, dynMethod.CallingConvention, dynMethod.ReturnType, dynParameters, null);
            }

            // Handle return value
            if (duckChaining)
            {
                // If we are in a duck chaining scenario we convert the field value to an object and push it to the stack
                ILHelpers.TypeConversion(il, returnType, typeof(object));

                // We call DuckType.GetInnerDuckType() with the 3 loaded values from the stack: ducktype field reference, property type and the, field value
                il.EmitCall(OpCodes.Call, GetInnerDuckTypeMethodInfo, null);
            }
            else if (returnType != proxyProperty.PropertyType)
            {
                // If the type is not the expected type we try a conversion.
                ILHelpers.TypeConversion(il, returnType, proxyProperty.PropertyType);
            }

            il.Emit(OpCodes.Ret);
            return proxyMethod;
        }

        private static MethodBuilder GetPropertySetMethod(TypeBuilder proxyTypeBuilder, Type targetType, PropertyInfo proxyProperty, PropertyInfo targetProperty, FieldInfo instanceField)
        {
            Type[] proxyParameterTypes = GetPropertySetParametersTypes(proxyProperty, true).ToArray();
            Type[] targetParametersTypes = GetPropertySetParametersTypes(targetProperty, true).ToArray();
            if (proxyParameterTypes.Length != targetParametersTypes.Length)
            {
                throw new DuckTypePropertyArgumentsLengthException(proxyProperty);
            }

            MethodBuilder proxyMethod = proxyTypeBuilder.DefineMethod(
                "set_" + proxyProperty.Name,
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.Virtual,
                typeof(void),
                proxyParameterTypes);

            ILGenerator il = proxyMethod.GetILGenerator();
            MethodInfo targetMethod = targetProperty.SetMethod;
            bool publicInstance = targetType.IsPublic || targetType.IsNestedPublic;

            // Load the instance if needed
            if (!targetMethod.IsStatic)
            {
                ILHelpers.LoadInstance(il, instanceField, targetType);
            }

            // Load the indexer keys and set value to the stack
            for (int pIndex = 0; pIndex < proxyParameterTypes.Length; pIndex++)
            {
                Type proxyParamType = proxyParameterTypes[pIndex];
                Type targetParamType = targetParametersTypes[pIndex];

                // Check if the type can be converted of if we need to enable duck chaining
                if (proxyParamType != targetParamType && !proxyParamType.IsValueType && !proxyParamType.IsAssignableFrom(targetProperty.PropertyType))
                {
                    // Create and load the duck type field reference to the stack
                    if (targetMethod.IsStatic)
                    {
                        FieldInfo innerField = DynamicFields.GetOrAdd(new VTuple<string, TypeBuilder>($"_duckStatic_{proxyProperty.Name}_{pIndex}", proxyTypeBuilder), tuple =>
                            tuple.Item2.DefineField(tuple.Item1, typeof(IDuckType), FieldAttributes.Private | FieldAttributes.Static));
                        il.Emit(OpCodes.Ldsflda, innerField);
                    }
                    else
                    {
                        FieldInfo innerField = DynamicFields.GetOrAdd(new VTuple<string, TypeBuilder>($"_duck_{proxyProperty.Name}_{pIndex}", proxyTypeBuilder), tuple =>
                            tuple.Item2.DefineField(tuple.Item1, typeof(IDuckType), FieldAttributes.Private));
                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Ldflda, innerField);
                    }

                    // Load the property type to the stack
                    il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Castclass, typeof(IDuckType));
                    il.EmitCall(OpCodes.Call, SetInnerDuckTypeMethodInfo, null);
                    targetParamType = typeof(object);
                }
                else
                {
                    ILHelpers.WriteLoadArgument(pIndex, il, false);
                    targetParamType = targetParamType.IsPublic || targetParamType.IsNestedPublic ? targetParamType : typeof(object);
                    ILHelpers.TypeConversion(il, proxyParamType, targetParamType);
                }

                targetParametersTypes[pIndex] = targetParamType;
            }

            // Call the set method
            if (publicInstance)
            {
                // If the instance is public we can emit directly without any dynamic method

                if (targetMethod.IsPublic)
                {
                    // We can emit a normal call if we have a public instance with a public property method.
                    il.EmitCall(targetMethod.IsStatic ? OpCodes.Call : OpCodes.Callvirt, targetMethod, null);
                }
                else
                {
                    // In case we have a public instance and a non public property method we can use [Calli] with the function pointer
                    il.Emit(OpCodes.Ldc_I8, (long)targetMethod.MethodHandle.GetFunctionPointer());
                    il.Emit(OpCodes.Conv_I);
                    il.EmitCalli(
                        OpCodes.Calli,
                        targetMethod.CallingConvention,
                        targetMethod.ReturnType,
                        targetMethod.GetParameters().Select(p => p.ParameterType).ToArray(),
                        null);
                }
            }
            else
            {
                // If the instance is not public we need to create a Dynamic method to overpass the visibility checks
                // we can't access non public types so we have to cast to object type (in the instance object and the return type).

                string dynMethodName = $"_setNonPublicProperty+{targetProperty.DeclaringType.Name}.{targetProperty.Name}";

                // We create the dynamic method
                Type[] targetParameters = GetPropertySetParametersTypes(targetProperty, true, !targetMethod.IsStatic).ToArray();
                Type[] dynParameters = targetMethod.IsStatic ? targetParametersTypes : (new[] { typeof(object) }).Concat(targetParametersTypes).ToArray();
                DynamicMethod dynMethod = new DynamicMethod(dynMethodName, typeof(void), dynParameters, typeof(DuckType).Module, true);

                // We store the dynamic method in a bag to avoid getting collected by the GC.
                DynamicMethods.Add(dynMethod);

                // Emit the dynamic method body
                ILGenerator dynIL = dynMethod.GetILGenerator();

                if (!targetMethod.IsStatic)
                {
                    dynIL.Emit(OpCodes.Ldarg_0);
                    if (targetProperty.DeclaringType != targetType)
                    {
                        dynIL.Emit(OpCodes.Castclass, targetProperty.DeclaringType);
                    }
                }

                for (int idx = targetMethod.IsStatic ? 0 : 1; idx < dynParameters.Length; idx++)
                {
                    ILHelpers.WriteLoadArgument(idx, dynIL, true);
                    ILHelpers.TypeConversion(dynIL, dynParameters[idx], targetParameters[idx]);
                }

                dynIL.EmitCall(targetMethod.IsStatic ? OpCodes.Call : OpCodes.Callvirt, targetMethod, null);
                dynIL.Emit(OpCodes.Ret);

                // Emit the Call to the dynamic method pointer [Calli]
                il.Emit(OpCodes.Ldc_I8, (long)GetRuntimeHandle(dynMethod).GetFunctionPointer());
                il.Emit(OpCodes.Conv_I);
                il.EmitCalli(OpCodes.Calli, dynMethod.CallingConvention, dynMethod.ReturnType, dynParameters, null);
            }

            il.Emit(OpCodes.Ret);
            return proxyMethod;
        }

        private static IEnumerable<Type> GetPropertyGetParametersTypes(PropertyInfo property, bool originalTypes, bool isDynamicSignature = false)
        {
            if (isDynamicSignature)
            {
                yield return typeof(object);
            }

            ParameterInfo[] idxParams = property.GetIndexParameters();
            foreach (ParameterInfo parameter in idxParams)
            {
                if (originalTypes || property.PropertyType.IsPublic || property.PropertyType.IsNestedPublic)
                {
                    yield return parameter.ParameterType;
                }
                else
                {
                    yield return typeof(object);
                }
            }
        }

        private static IEnumerable<Type> GetPropertySetParametersTypes(PropertyInfo property, bool originalTypes, bool isDynamicSignature = false)
        {
            if (isDynamicSignature)
            {
                yield return typeof(object);
            }

            foreach (Type indexType in GetPropertyGetParametersTypes(property, originalTypes))
            {
                yield return indexType;
            }

            if (originalTypes || property.PropertyType.IsPublic || property.PropertyType.IsNestedPublic)
            {
                yield return property.PropertyType;
            }
            else
            {
                yield return typeof(object);
            }
        }
    }
}
