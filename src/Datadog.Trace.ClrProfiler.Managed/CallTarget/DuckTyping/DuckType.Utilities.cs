using System;

namespace Datadog.Trace.ClrProfiler.CallTarget.DuckTyping
{
    /// <summary>
    /// Duck Type
    /// </summary>
    public partial class DuckType
    {
        /// <summary>
        /// Checks and ensures the arguments for the Create methods
        /// </summary>
        /// <param name="interfaceType">Interface type</param>
        /// <param name="instance">Instance value</param>
        /// <exception cref="ArgumentNullException">If the interface type or the instance value is null</exception>
        /// <exception cref="ArgumentException">If the interface type is not an interface or is neither public or nested public</exception>
        private static void EnsureArguments(Type interfaceType, object instance)
        {
            if (interfaceType is null)
            {
                throw new ArgumentNullException(nameof(interfaceType), "The interface type can't be null");
            }

            if (instance is null)
            {
                throw new ArgumentNullException(nameof(instance), "The object instance can't be null");
            }

            if (!interfaceType.IsPublic && !interfaceType.IsNestedPublic)
            {
                throw new DuckTypeTypeIsNotPublicException(interfaceType, nameof(interfaceType));
            }
        }

        /// <summary>
        /// Get inner DuckType
        /// </summary>
        /// <param name="field">Field reference</param>
        /// <param name="interfaceType">Interface type</param>
        /// <param name="value">Property value</param>
        /// <returns>DuckType instance</returns>
        protected static IDuckType GetInnerDuckType(ref DuckType field, Type interfaceType, object value)
        {
            if (value is null)
            {
                field = null;
                return null;
            }

            var valueType = value.GetType();
            if (field is null || field.Type != valueType)
            {
                field = (DuckType)Create(interfaceType, valueType);
            }

            field.SetInstance(value);
            return (IDuckType)field;
        }

        /// <summary>
        /// Set inner DuckType
        /// </summary>
        /// <param name="field">Field reference</param>
        /// <param name="value">DuckType instance</param>
        /// <returns>Property value</returns>
        protected static object SetInnerDuckType(ref DuckType field, DuckType value)
        {
            field = value;
            return field?.Instance;
        }
    }
}
