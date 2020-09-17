using System;
using System.Reflection;
#pragma warning disable SA1649 // File name must match first type name
#pragma warning disable SA1402 // File may only contain a single class

namespace Datadog.Trace.ClrProfiler.DuckTyping
{
    /// <summary>
    /// DuckType Exception
    /// </summary>
    public class DuckTypeException : Exception
    {
        internal DuckTypeException(string message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// DuckType property can't be read
    /// </summary>
    public class DuckTypePropertyCantBeReadException : DuckTypeException
    {
        internal DuckTypePropertyCantBeReadException(PropertyInfo property)
            : base($"The property '{property.Name}' can't be read, you should remove the getter from the base type class or interface.")
        {
        }
    }

    /// <summary>
    /// DuckType property can't be written
    /// </summary>
    public class DuckTypePropertyCantBeWrittenException : DuckTypeException
    {
        internal DuckTypePropertyCantBeWrittenException(PropertyInfo property)
            : base($"The property '{property.Name}' can't be written, you should remove the setter from the base type class or interface.")
        {
        }
    }

    /// <summary>
    /// DuckType property argument doesn't have the same argument length
    /// </summary>
    public class DuckTypePropertyArgumentsLengthException : DuckTypeException
    {
        internal DuckTypePropertyArgumentsLengthException(PropertyInfo property)
            : base($"The property '{property.Name}' doesn't have the same number of arguments as the original property.")
        {
        }
    }

    /// <summary>
    /// DuckType field is readonly
    /// </summary>
    public class DuckTypeFieldIsReadonlyException : DuckTypeException
    {
        internal DuckTypeFieldIsReadonlyException(FieldInfo field)
            : base($"The field '{field.Name}' is marked as readonly, you should remove the setter from the base type class or interface.")
        {
        }
    }

    /// <summary>
    /// DuckType property or field not found
    /// </summary>
    public class DuckTypePropertyOrFieldNotFoundException : DuckTypeException
    {
        internal DuckTypePropertyOrFieldNotFoundException(string name)
            : base($"The property or field  for '{name}' was not found in the instance.")
        {
        }
    }

    /// <summary>
    /// DuckType type is not an interface exception
    /// </summary>
    public class DuckTypeTypeIsNotValidException : DuckTypeException
    {
        internal DuckTypeTypeIsNotValidException(Type type, string argumentName)
            : base($"The type '{type.FullName}' is not a valid type, argument: '{argumentName}'")
        {
        }
    }

    /// <summary>
    /// DuckType type is not public exception
    /// </summary>
    public class DuckTypeTypeIsNotPublicException : DuckTypeException
    {
        internal DuckTypeTypeIsNotPublicException(Type type, string argumentName)
            : base($"The type '{type.FullName}' must be public, argument: '{argumentName}'")
        {
        }
    }

    /// <summary>
    /// DuckType struct members cannot be changed exception
    /// </summary>
    public class DuckTypeStructMembersCannotBeChangedException : DuckTypeException
    {
        internal DuckTypeStructMembersCannotBeChangedException(Type type)
            : base($"Modifying struct members is not supported. [{type.FullName}]")
        {
        }
    }

    /// <summary>
    /// DuckType target method can not be found exception
    /// </summary>
    public class DuckTypeTargetMethodNotFoundException : DuckTypeException
    {
        internal DuckTypeTargetMethodNotFoundException(MethodInfo method)
            : base($"The target method for the proxy method '{method.ToString()}' was not found.")
        {
        }
    }
}
