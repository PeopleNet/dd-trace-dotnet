using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Datadog.Trace.ClrProfiler.DuckTyping
{
    /// <summary>
    /// Internal IL Helpers
    /// </summary>
    internal static class ILHelpers
    {
        private static readonly OpCode[] EmptyOpcodes = new OpCode[0];
        private static readonly OpCode[] ConvI1Opcode = new[] { OpCodes.Conv_I1 };
        private static readonly OpCode[] ConvU1Opcode = new[] { OpCodes.Conv_U1 };
        private static readonly OpCode[] ConvI2Opcode = new[] { OpCodes.Conv_I2 };
        private static readonly OpCode[] ConvU2Opcode = new[] { OpCodes.Conv_U2 };
        private static readonly OpCode[] ConvI4Opcode = new[] { OpCodes.Conv_I4 };
        private static readonly OpCode[] ConvU4Opcode = new[] { OpCodes.Conv_U4 };
        private static readonly OpCode[] ConvI8Opcode = new[] { OpCodes.Conv_I8 };
        private static readonly OpCode[] ConvU8Opcode = new[] { OpCodes.Conv_U8 };
        private static readonly OpCode[] ConvR4Opcode = new[] { OpCodes.Conv_R4 };
        private static readonly OpCode[] ConvR8Opcode = new[] { OpCodes.Conv_R8 };
        private static readonly OpCode[] ConvRUnR4Opcode = new[] { OpCodes.Conv_R_Un, OpCodes.Conv_R4 };
        private static readonly OpCode[] ConvRUnR8Opcode = new[] { OpCodes.Conv_R_Un, OpCodes.Conv_R8 };
        private static readonly Dictionary<VTuple<Type, Type>, OpCode[]> ConversionTable = new Dictionary<VTuple<Type, Type>, OpCode[]>
        {
            { new VTuple<Type, Type>(typeof(double), typeof(sbyte)), ConvI1Opcode },
            { new VTuple<Type, Type>(typeof(float), typeof(sbyte)), ConvI1Opcode },
            { new VTuple<Type, Type>(typeof(char), typeof(sbyte)), ConvI1Opcode },
            { new VTuple<Type, Type>(typeof(ulong), typeof(sbyte)), ConvI1Opcode },
            { new VTuple<Type, Type>(typeof(long), typeof(sbyte)), ConvI1Opcode },
            { new VTuple<Type, Type>(typeof(uint), typeof(sbyte)), ConvI1Opcode },
            { new VTuple<Type, Type>(typeof(int), typeof(sbyte)), ConvI1Opcode },
            { new VTuple<Type, Type>(typeof(ushort), typeof(sbyte)), ConvI1Opcode },
            { new VTuple<Type, Type>(typeof(short), typeof(sbyte)), ConvI1Opcode },
            { new VTuple<Type, Type>(typeof(byte), typeof(sbyte)), ConvI1Opcode },
            // ...
            { new VTuple<Type, Type>(typeof(double), typeof(byte)), ConvU1Opcode },
            { new VTuple<Type, Type>(typeof(float), typeof(byte)), ConvU1Opcode },
            { new VTuple<Type, Type>(typeof(char), typeof(byte)), ConvU1Opcode },
            { new VTuple<Type, Type>(typeof(ulong), typeof(byte)), ConvU1Opcode },
            { new VTuple<Type, Type>(typeof(long), typeof(byte)), ConvU1Opcode },
            { new VTuple<Type, Type>(typeof(uint), typeof(byte)), ConvU1Opcode },
            { new VTuple<Type, Type>(typeof(int), typeof(byte)), ConvU1Opcode },
            { new VTuple<Type, Type>(typeof(ushort), typeof(byte)), ConvU1Opcode },
            { new VTuple<Type, Type>(typeof(short), typeof(byte)), ConvU1Opcode },
            { new VTuple<Type, Type>(typeof(sbyte), typeof(byte)), ConvU1Opcode },
            // ...
            { new VTuple<Type, Type>(typeof(double), typeof(short)), ConvI2Opcode },
            { new VTuple<Type, Type>(typeof(float), typeof(short)), ConvI2Opcode },
            { new VTuple<Type, Type>(typeof(char), typeof(short)), ConvI2Opcode },
            { new VTuple<Type, Type>(typeof(ulong), typeof(short)), ConvI2Opcode },
            { new VTuple<Type, Type>(typeof(long), typeof(short)), ConvI2Opcode },
            { new VTuple<Type, Type>(typeof(uint), typeof(short)), ConvI2Opcode },
            { new VTuple<Type, Type>(typeof(int), typeof(short)), ConvI2Opcode },
            { new VTuple<Type, Type>(typeof(ushort), typeof(short)), ConvI2Opcode },
            { new VTuple<Type, Type>(typeof(byte), typeof(short)), EmptyOpcodes },
            { new VTuple<Type, Type>(typeof(sbyte), typeof(short)), EmptyOpcodes },
            // ...
            { new VTuple<Type, Type>(typeof(double), typeof(ushort)), ConvU2Opcode },
            { new VTuple<Type, Type>(typeof(float), typeof(ushort)), ConvU2Opcode },
            { new VTuple<Type, Type>(typeof(char), typeof(ushort)), EmptyOpcodes },
            { new VTuple<Type, Type>(typeof(ulong), typeof(ushort)), ConvU2Opcode },
            { new VTuple<Type, Type>(typeof(long), typeof(ushort)), ConvU2Opcode },
            { new VTuple<Type, Type>(typeof(uint), typeof(ushort)), ConvU2Opcode },
            { new VTuple<Type, Type>(typeof(int), typeof(ushort)), ConvU2Opcode },
            { new VTuple<Type, Type>(typeof(short), typeof(ushort)), ConvU2Opcode },
            { new VTuple<Type, Type>(typeof(byte), typeof(ushort)), EmptyOpcodes },
            { new VTuple<Type, Type>(typeof(sbyte), typeof(ushort)), EmptyOpcodes },
            // ...
            { new VTuple<Type, Type>(typeof(double), typeof(int)), ConvI4Opcode },
            { new VTuple<Type, Type>(typeof(float), typeof(int)), ConvI4Opcode },
            { new VTuple<Type, Type>(typeof(char), typeof(int)), EmptyOpcodes },
            { new VTuple<Type, Type>(typeof(ulong), typeof(int)), ConvI4Opcode },
            { new VTuple<Type, Type>(typeof(long), typeof(int)), ConvI4Opcode },
            { new VTuple<Type, Type>(typeof(uint), typeof(int)), ConvI4Opcode },
            { new VTuple<Type, Type>(typeof(ushort), typeof(int)), EmptyOpcodes },
            { new VTuple<Type, Type>(typeof(short), typeof(int)), EmptyOpcodes },
            { new VTuple<Type, Type>(typeof(byte), typeof(int)), EmptyOpcodes },
            { new VTuple<Type, Type>(typeof(sbyte), typeof(int)), EmptyOpcodes },
            // ...
            { new VTuple<Type, Type>(typeof(double), typeof(uint)), ConvU4Opcode },
            { new VTuple<Type, Type>(typeof(float), typeof(uint)), ConvU4Opcode },
            { new VTuple<Type, Type>(typeof(char), typeof(uint)), EmptyOpcodes },
            { new VTuple<Type, Type>(typeof(ulong), typeof(uint)), ConvU4Opcode },
            { new VTuple<Type, Type>(typeof(long), typeof(uint)), ConvU4Opcode },
            { new VTuple<Type, Type>(typeof(int), typeof(uint)), EmptyOpcodes },
            { new VTuple<Type, Type>(typeof(ushort), typeof(uint)), EmptyOpcodes },
            { new VTuple<Type, Type>(typeof(short), typeof(uint)), EmptyOpcodes },
            { new VTuple<Type, Type>(typeof(byte), typeof(uint)), EmptyOpcodes },
            { new VTuple<Type, Type>(typeof(sbyte), typeof(uint)), EmptyOpcodes },
            // ...
            { new VTuple<Type, Type>(typeof(double), typeof(long)), ConvI8Opcode },
            { new VTuple<Type, Type>(typeof(float), typeof(long)), ConvI8Opcode },
            { new VTuple<Type, Type>(typeof(char), typeof(long)), ConvU8Opcode },
            { new VTuple<Type, Type>(typeof(ulong), typeof(long)), EmptyOpcodes },
            { new VTuple<Type, Type>(typeof(uint), typeof(long)), ConvU8Opcode },
            { new VTuple<Type, Type>(typeof(int), typeof(long)), ConvI8Opcode },
            { new VTuple<Type, Type>(typeof(ushort), typeof(long)), ConvU8Opcode },
            { new VTuple<Type, Type>(typeof(short), typeof(long)), ConvI8Opcode },
            { new VTuple<Type, Type>(typeof(byte), typeof(long)), ConvU8Opcode },
            { new VTuple<Type, Type>(typeof(sbyte), typeof(long)), ConvI8Opcode },
            // ...
            { new VTuple<Type, Type>(typeof(double), typeof(ulong)), ConvU8Opcode },
            { new VTuple<Type, Type>(typeof(float), typeof(ulong)), ConvU8Opcode },
            { new VTuple<Type, Type>(typeof(char), typeof(ulong)), ConvU8Opcode },
            { new VTuple<Type, Type>(typeof(long), typeof(ulong)), EmptyOpcodes },
            { new VTuple<Type, Type>(typeof(uint), typeof(ulong)), ConvU8Opcode },
            { new VTuple<Type, Type>(typeof(int), typeof(ulong)), ConvI8Opcode },
            { new VTuple<Type, Type>(typeof(ushort), typeof(ulong)), ConvU8Opcode },
            { new VTuple<Type, Type>(typeof(short), typeof(ulong)), ConvI8Opcode },
            { new VTuple<Type, Type>(typeof(byte), typeof(ulong)), ConvU8Opcode },
            { new VTuple<Type, Type>(typeof(sbyte), typeof(ulong)), ConvI8Opcode },
            // ...
            { new VTuple<Type, Type>(typeof(double), typeof(char)), ConvU2Opcode },
            { new VTuple<Type, Type>(typeof(float), typeof(char)), ConvU2Opcode },
            { new VTuple<Type, Type>(typeof(ulong), typeof(char)), ConvU2Opcode },
            { new VTuple<Type, Type>(typeof(long), typeof(char)), ConvU2Opcode },
            { new VTuple<Type, Type>(typeof(uint), typeof(char)), ConvU2Opcode },
            { new VTuple<Type, Type>(typeof(int), typeof(char)), ConvU2Opcode },
            { new VTuple<Type, Type>(typeof(ushort), typeof(char)), EmptyOpcodes },
            { new VTuple<Type, Type>(typeof(short), typeof(char)), ConvU2Opcode },
            { new VTuple<Type, Type>(typeof(byte), typeof(char)), EmptyOpcodes },
            { new VTuple<Type, Type>(typeof(sbyte), typeof(char)), ConvU2Opcode },
            // ...
            { new VTuple<Type, Type>(typeof(double), typeof(float)), ConvR4Opcode },
            { new VTuple<Type, Type>(typeof(char), typeof(float)), ConvR4Opcode },
            { new VTuple<Type, Type>(typeof(ulong), typeof(float)), ConvRUnR4Opcode },
            { new VTuple<Type, Type>(typeof(long), typeof(float)), ConvR4Opcode },
            { new VTuple<Type, Type>(typeof(uint), typeof(float)), ConvRUnR4Opcode },
            { new VTuple<Type, Type>(typeof(int), typeof(float)), ConvR4Opcode },
            { new VTuple<Type, Type>(typeof(ushort), typeof(float)), ConvR4Opcode },
            { new VTuple<Type, Type>(typeof(short), typeof(float)), ConvR4Opcode },
            { new VTuple<Type, Type>(typeof(byte), typeof(float)), ConvR4Opcode },
            { new VTuple<Type, Type>(typeof(sbyte), typeof(float)), ConvR4Opcode },
            // ...
            { new VTuple<Type, Type>(typeof(float), typeof(double)), ConvR8Opcode },
            { new VTuple<Type, Type>(typeof(char), typeof(double)), ConvR8Opcode },
            { new VTuple<Type, Type>(typeof(ulong), typeof(double)), ConvRUnR8Opcode },
            { new VTuple<Type, Type>(typeof(long), typeof(double)), ConvR8Opcode },
            { new VTuple<Type, Type>(typeof(uint), typeof(double)), ConvRUnR8Opcode },
            { new VTuple<Type, Type>(typeof(int), typeof(double)), ConvR8Opcode },
            { new VTuple<Type, Type>(typeof(ushort), typeof(double)), ConvR8Opcode },
            { new VTuple<Type, Type>(typeof(short), typeof(double)), ConvR8Opcode },
            { new VTuple<Type, Type>(typeof(byte), typeof(double)), ConvR8Opcode },
            { new VTuple<Type, Type>(typeof(sbyte), typeof(double)), ConvR8Opcode },
        };

        /// <summary>
        /// Load instance field
        /// </summary>
        /// <param name="il">IlGenerator</param>
        /// <param name="instanceField">Instance field</param>
        /// <param name="instanceType">Instance type</param>
        internal static void LoadInstance(ILGenerator il, FieldInfo instanceField, Type instanceType)
        {
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, instanceField);
        }

        /// <summary>
        /// Load instance argument
        /// </summary>
        /// <param name="il">ILGenerator</param>
        /// <param name="actualType">Actual type</param>
        /// <param name="expectedType">Expected type</param>
        internal static void LoadInstanceArgument(ILGenerator il, Type actualType, Type expectedType)
        {
            il.Emit(OpCodes.Ldarg_0);
            if (actualType == expectedType)
            {
                return;
            }

            if (expectedType.IsValueType)
            {
                il.DeclareLocal(expectedType);
                il.Emit(OpCodes.Unbox_Any, expectedType);
                il.Emit(OpCodes.Stloc_0);
                il.Emit(OpCodes.Ldloca_S, 0);
            }
            else
            {
                il.Emit(OpCodes.Castclass, expectedType);
            }
        }

        /// <summary>
        /// Write load arguments
        /// </summary>
        /// <param name="index">Argument index</param>
        /// <param name="il">IlGenerator</param>
        /// <param name="isStatic">Define if we need to take into account the instance argument</param>
        internal static void WriteLoadArgument(int index, ILGenerator il, bool isStatic)
        {
            switch (index)
            {
                case 0:
                    il.Emit(isStatic ? OpCodes.Ldarg_0 : OpCodes.Ldarg_1);
                    break;
                case 1:
                    il.Emit(isStatic ? OpCodes.Ldarg_1 : OpCodes.Ldarg_2);
                    break;
                case 2:
                    il.Emit(isStatic ? OpCodes.Ldarg_2 : OpCodes.Ldarg_3);
                    break;
                case 3:
                    if (isStatic)
                    {
                        il.Emit(OpCodes.Ldarg_3);
                    }
                    else
                    {
                        il.Emit(OpCodes.Ldarg_S, 4);
                    }

                    break;
                default:
                    il.Emit(OpCodes.Ldarg_S, isStatic ? index : index + 1);
                    break;
            }
        }

        /// <summary>
        /// Write int value
        /// </summary>
        /// <param name="il">ILGenerator</param>
        /// <param name="value">Integer value</param>
        internal static void WriteIlIntValue(ILGenerator il, int value)
        {
            switch (value)
            {
                case 0:
                    il.Emit(OpCodes.Ldc_I4_0);
                    break;
                case 1:
                    il.Emit(OpCodes.Ldc_I4_1);
                    break;
                case 2:
                    il.Emit(OpCodes.Ldc_I4_2);
                    break;
                case 3:
                    il.Emit(OpCodes.Ldc_I4_3);
                    break;
                case 4:
                    il.Emit(OpCodes.Ldc_I4_4);
                    break;
                case 5:
                    il.Emit(OpCodes.Ldc_I4_5);
                    break;
                case 6:
                    il.Emit(OpCodes.Ldc_I4_6);
                    break;
                case 7:
                    il.Emit(OpCodes.Ldc_I4_7);
                    break;
                case 8:
                    il.Emit(OpCodes.Ldc_I4_8);
                    break;
                default:
                    il.Emit(OpCodes.Ldc_I4_S, value);
                    break;
            }
        }

        /// <summary>
        /// Convert a current type to an expected type
        /// </summary>
        /// <param name="il">ILGenerator</param>
        /// <param name="actualType">Actual type</param>
        /// <param name="expectedType">Expected type</param>
        internal static void TypeConversion(ILGenerator il, Type actualType, Type expectedType)
        {
            if (actualType == expectedType)
            {
                return;
            }

            var actualUnderlyingType = actualType.IsEnum ? Enum.GetUnderlyingType(actualType) : actualType;
            var expectedUnderlyingType = expectedType.IsEnum ? Enum.GetUnderlyingType(expectedType) : expectedType;

            if (actualUnderlyingType.IsValueType)
            {
                if (expectedUnderlyingType.IsValueType)
                {
                    if (ConvertValueTypes(il, actualUnderlyingType, expectedUnderlyingType))
                    {
                        return;
                    }

                    il.Emit(OpCodes.Box, actualUnderlyingType);
                    il.Emit(OpCodes.Unbox_Any, expectedUnderlyingType);
                }
                else
                {
                    il.Emit(OpCodes.Box, actualType);
                    il.Emit(OpCodes.Castclass, expectedType);
                }
            }
            else
            {
                if (expectedType.IsValueType)
                {
                    il.Emit(OpCodes.Ldtoken, expectedUnderlyingType);
                    il.EmitCall(OpCodes.Call, Util.GetTypeFromHandleMethodInfo, null);
                    il.EmitCall(OpCodes.Call, Util.ConvertTypeMethodInfo, null);
                    il.Emit(OpCodes.Unbox_Any, expectedType);
                }
                else if (expectedType != typeof(object))
                {
                    il.Emit(OpCodes.Castclass, expectedType);
                }
            }
        }

        /// <summary>
        /// Converts basic value types using the conversion OpCodes
        /// </summary>
        /// <param name="il">ILGenerator</param>
        /// <param name="currentType">Current value type</param>
        /// <param name="expectedType">Expected value type</param>
        /// <returns>True if the conversion was made; otherwise, false</returns>
        private static bool ConvertValueTypes(ILGenerator il, Type currentType, Type expectedType)
        {
            if (currentType == expectedType)
            {
                return true;
            }

            if (ConversionTable.TryGetValue(new VTuple<Type, Type>(currentType, expectedType), out OpCode[] codes))
            {
                foreach (OpCode opCode in codes)
                {
                    il.Emit(opCode);
                }

                return true;
            }

            return false;
        }
    }
}
