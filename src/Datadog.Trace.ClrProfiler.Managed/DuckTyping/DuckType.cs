using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.ExceptionServices;
using System.Security.Cryptography;

namespace Datadog.Trace.ClrProfiler.DuckTyping
{
    /// <summary>
    /// Duck Type
    /// </summary>
    public static partial class DuckType
    {
        /// <summary>
        /// Gets or create a new proxy type for ducktyping
        /// </summary>
        /// <param name="proxyType">ProxyType interface</param>
        /// <param name="targetType">Target type</param>
        /// <returns>CreateTypeResult instance</returns>
        internal static CreateTypeResult GetOrCreateProxyType(Type proxyType, Type targetType)
        {
            VTuple<Type, Type> key = new VTuple<Type, Type>(proxyType, targetType);

            if (DuckTypeCache.TryGetValue(key, out CreateTypeResult proxyTypeResult))
            {
                return proxyTypeResult;
            }

            lock (DuckTypeCache)
            {
                if (!DuckTypeCache.TryGetValue(key, out proxyTypeResult))
                {
                    proxyTypeResult = CreateProxyType(proxyType, targetType);
                    DuckTypeCache[key] = proxyTypeResult;
                }

                return proxyTypeResult;
            }
        }

        private static CreateTypeResult CreateProxyType(Type proxyType, Type targetType)
        {
            try
            {
                // Define parent type, interface types
                Type parentType;
                TypeAttributes typeAttributes;
                Type[] interfaceTypes;
                if (proxyType.IsInterface)
                {
                    // If the proxy type definition is an interface we create an struct proxy
                    parentType = typeof(ValueType);
                    typeAttributes = TypeAttributes.Public | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit | TypeAttributes.SequentialLayout | TypeAttributes.Sealed | TypeAttributes.Serializable;
                    interfaceTypes = new[] { proxyType, typeof(IDuckType) };
                }
                else
                {
                    // If the proxy type definition is a class then we create a class proxy
                    parentType = proxyType;
                    typeAttributes = TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit | TypeAttributes.AutoLayout | TypeAttributes.Sealed;
                    interfaceTypes = new[] { typeof(IDuckTypeClass) };
                }

                // Ensures the module builder
                if (_moduleBuilder is null)
                {
                    lock (_locker)
                    {
                        if (_moduleBuilder is null)
                        {
                            AssemblyName aName = new AssemblyName("DuckTypeAssembly");
                            AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(aName, AssemblyBuilderAccess.Run);
                            _moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");
                        }
                    }
                }

                string proxyTypeName = $"{proxyType.FullName.Replace(".", "_").Replace("+", "__")}___{targetType.FullName.Replace(".", "_").Replace("+", "__")}";
                Log.Information("Creating type proxy: " + proxyTypeName);

                // Create Type
                TypeBuilder proxyTypeBuilder = _moduleBuilder.DefineType(
                    proxyTypeName,
                    typeAttributes,
                    parentType,
                    interfaceTypes);

                // Create IDuckType and IDuckTypeSetter implementations
                FieldInfo instanceField = CreateIDuckTypeImplementation(proxyTypeBuilder, !proxyType.IsInterface);

                // Define .ctor
                ConstructorBuilder ctorBuilder = proxyTypeBuilder.DefineConstructor(
                    MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
                    CallingConventions.Standard,
                    TypeObjectArray);
                ILGenerator ctorIL = ctorBuilder.GetILGenerator();
                ctorIL.Emit(OpCodes.Ldarg_0);
                ctorIL.Emit(OpCodes.Ldarg_1);
                ctorIL.Emit(OpCodes.Stfld, instanceField);
                ctorIL.Emit(OpCodes.Ret);

                // Create Members
                CreateProperties(proxyTypeBuilder, proxyType, targetType, instanceField);
                CreateMethods(proxyTypeBuilder, proxyType, targetType, instanceField);

                // Create Type
                return new CreateTypeResult(proxyTypeBuilder.CreateTypeInfo().AsType(), null);
            }
            catch (Exception ex)
            {
                return new CreateTypeResult(null, ExceptionDispatchInfo.Capture(ex));
            }
        }

        private static FieldInfo CreateIDuckTypeImplementation(TypeBuilder proxyTypeBuilder, bool emitSetInstance)
        {
            var instanceField = proxyTypeBuilder.DefineField("_currentInstance", typeof(object), FieldAttributes.Private | FieldAttributes.InitOnly);

            var propInstance = proxyTypeBuilder.DefineProperty("Instance", PropertyAttributes.None, typeof(object), null);
            var getPropInstance = proxyTypeBuilder.DefineMethod(
                "get_Instance",
                MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot,
                typeof(object),
                Type.EmptyTypes);
            ILGenerator il = getPropInstance.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, instanceField);
            il.Emit(OpCodes.Ret);
            propInstance.SetGetMethod(getPropInstance);

            var propType = proxyTypeBuilder.DefineProperty("Type", PropertyAttributes.None, typeof(Type), null);
            var getPropType = proxyTypeBuilder.DefineMethod(
                "get_Type",
                MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot,
                typeof(Type),
                Type.EmptyTypes);
            il = getPropType.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, instanceField);
            il.EmitCall(OpCodes.Callvirt, typeof(object).GetMethod("GetType"), null);
            il.Emit(OpCodes.Ret);
            propType.SetGetMethod(getPropType);

            var propVersion = proxyTypeBuilder.DefineProperty("AssemblyVersion", PropertyAttributes.None, typeof(Version), null);
            var getPropVersion = proxyTypeBuilder.DefineMethod(
                "get_AssemblyVersion",
                MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot,
                typeof(Version),
                Type.EmptyTypes);
            il = getPropVersion.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, instanceField);
            il.EmitCall(OpCodes.Call, typeof(object).GetMethod("GetType"), null);
            il.EmitCall(OpCodes.Callvirt, typeof(Type).GetProperty("Assembly").GetMethod, null);
            il.EmitCall(OpCodes.Callvirt, typeof(Assembly).GetMethod("GetName", Type.EmptyTypes), null);
            il.EmitCall(OpCodes.Callvirt, typeof(AssemblyName).GetProperty("Version").GetMethod, null);
            il.Emit(OpCodes.Ret);
            propVersion.SetGetMethod(getPropVersion);

            if (emitSetInstance)
            {
                var setInstance = proxyTypeBuilder.DefineMethod(
                    "SetInstance",
                    MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot,
                    typeof(void),
                    new[] { typeof(object) });
                il = setInstance.GetILGenerator();
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Stfld, instanceField);
                il.Emit(OpCodes.Ret);
            }

            return instanceField;
        }

        private static List<PropertyInfo> GetProperties(Type proxyType)
        {
            var selectedProperties = new List<PropertyInfo>(proxyType.IsInterface ? proxyType.GetProperties() : GetBaseProperties(proxyType));
            var implementedInterfaces = proxyType.GetInterfaces();
            foreach (var imInterface in implementedInterfaces)
            {
                if (imInterface == typeof(IDuckType) || imInterface == typeof(IDuckTypeClass))
                {
                    continue;
                }

                var newProps = imInterface.GetProperties().Where(p => selectedProperties.All(i => i.Name != p.Name));
                selectedProperties.AddRange(newProps);
            }

            return selectedProperties;

            static IEnumerable<PropertyInfo> GetBaseProperties(Type baseType)
            {
                foreach (var prop in baseType.GetProperties())
                {
                    if (prop.DeclaringType == typeof(DuckType))
                    {
                        continue;
                    }

                    if (prop.CanRead && (prop.GetMethod.IsAbstract || prop.GetMethod.IsVirtual))
                    {
                        yield return prop;
                    }
                    else if (prop.CanWrite && (prop.SetMethod.IsAbstract || prop.SetMethod.IsVirtual))
                    {
                        yield return prop;
                    }
                }
            }
        }

        private static void CreateProperties(TypeBuilder proxyTypeBuilder, Type proxyType, Type targetType, FieldInfo instanceField)
        {
            var targetTypeAssemblyVersion = targetType.Assembly.GetName().Version;
            // Gets all properties to be implemented
            var proxyTypeProperties = GetProperties(proxyType);

            foreach (var proxyProperty in proxyTypeProperties)
            {
                PropertyBuilder propertyBuilder = null;

                // If the property is abstract or interface we make sure that we have the property defined in the new class
                if ((proxyProperty.CanRead && proxyProperty.GetMethod.IsAbstract) || (proxyProperty.CanWrite && proxyProperty.SetMethod.IsAbstract))
                {
                    propertyBuilder = proxyTypeBuilder.DefineProperty(proxyProperty.Name, PropertyAttributes.None, proxyProperty.PropertyType, null);
                }

                var duckAttrs = new List<DuckAttribute>(proxyProperty.GetCustomAttributes<DuckAttribute>(true));
                if (duckAttrs.Count == 0)
                {
                    duckAttrs.Add(new DuckAttribute());
                }

                duckAttrs.Sort((x, y) =>
                {
                    if (x.Version is null)
                    {
                        return 1;
                    }

                    if (y.Version is null)
                    {
                        return -1;
                    }

                    return x.Version.CompareTo(y.Version);
                });

                foreach (var duckAttr in duckAttrs)
                {
                    if (!(duckAttr.Version is null) && targetTypeAssemblyVersion > duckAttr.Version)
                    {
                        continue;
                    }

                    duckAttr.Name ??= proxyProperty.Name;

                    switch (duckAttr.Kind)
                    {
                        case DuckKind.Property:
                            PropertyInfo targetProperty = null;
                            try
                            {
                                targetProperty = targetType.GetProperty(duckAttr.Name, duckAttr.BindingFlags);
                            }
                            catch
                            {
                                targetProperty = targetType.GetProperty(duckAttr.Name, proxyProperty.PropertyType, proxyProperty.GetIndexParameters().Select(i => i.ParameterType).ToArray());
                            }

                            if (targetProperty is null)
                            {
                                continue;
                            }

                            propertyBuilder ??= proxyTypeBuilder.DefineProperty(proxyProperty.Name, PropertyAttributes.None, proxyProperty.PropertyType, null);

                            if (proxyProperty.CanRead)
                            {
                                // Check if the target property can be read
                                if (!targetProperty.CanRead)
                                {
                                    throw new DuckTypePropertyCantBeReadException(targetProperty);
                                }

                                propertyBuilder.SetGetMethod(GetPropertyGetMethod(proxyTypeBuilder, targetType, proxyProperty, targetProperty, instanceField));
                            }

                            if (proxyProperty.CanWrite)
                            {
                                // Check if the target property can be written
                                if (!targetProperty.CanWrite)
                                {
                                    throw new DuckTypePropertyCantBeWrittenException(targetProperty);
                                }

                                // Check if the target property declaring type is an struct (structs modification is not supported)
                                if (targetProperty.DeclaringType.IsValueType)
                                {
                                    throw new DuckTypeStructMembersCannotBeChangedException(targetProperty.DeclaringType);
                                }

                                propertyBuilder.SetSetMethod(GetPropertySetMethod(proxyTypeBuilder, targetType, proxyProperty, targetProperty, instanceField));
                            }

                            break;

                        case DuckKind.Field:
                            FieldInfo targetField = targetType.GetField(duckAttr.Name, duckAttr.BindingFlags);
                            if (targetField is null)
                            {
                                continue;
                            }

                            propertyBuilder ??= proxyTypeBuilder.DefineProperty(proxyProperty.Name, PropertyAttributes.None, proxyProperty.PropertyType, null);

                            if (proxyProperty.CanRead)
                            {
                                propertyBuilder.SetGetMethod(GetFieldGetMethod(proxyTypeBuilder, targetType, proxyProperty, targetField, instanceField));
                            }

                            if (proxyProperty.CanWrite)
                            {
                                // Check if the target field is marked as InitOnly (readonly) and throw an exception in that case
                                if ((targetField.Attributes & FieldAttributes.InitOnly) != 0)
                                {
                                    throw new DuckTypeFieldIsReadonlyException(targetField);
                                }

                                // Check if the target field declaring type is an struct (structs modification is not supported)
                                if (targetField.DeclaringType.IsValueType)
                                {
                                    throw new DuckTypeStructMembersCannotBeChangedException(targetField.DeclaringType);
                                }

                                propertyBuilder.SetSetMethod(GetFieldSetMethod(proxyTypeBuilder, targetType, proxyProperty, targetField, instanceField));
                            }

                            break;
                    }

                    break;
                }

                if (propertyBuilder is null)
                {
                    continue;
                }

                if (proxyProperty.CanRead && propertyBuilder.GetMethod is null)
                {
                    throw new DuckTypePropertyOrFieldNotFoundException(proxyProperty.Name);
                }

                if (proxyProperty.CanWrite && propertyBuilder.SetMethod is null)
                {
                    throw new DuckTypePropertyOrFieldNotFoundException(proxyProperty.Name);
                }
            }
        }

        internal readonly struct CreateTypeResult
        {
            public readonly Type ProxyType;
            public readonly ExceptionDispatchInfo ExceptionInfo;
            public readonly bool Success;

            public CreateTypeResult(Type proxyType, ExceptionDispatchInfo exceptionInfo)
            {
                ProxyType = proxyType;
                ExceptionInfo = exceptionInfo;
                Success = proxyType != null && exceptionInfo == null;
            }
        }
    }
}
