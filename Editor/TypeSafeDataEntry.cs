using System;

namespace TypeSafe.Editor
{
    /// <summary>
    /// A data entry within a <c>TypeSafeDataUnit</c>
    /// </summary>
    public class TypeSafeDataEntry
    {
        /// <summary>
        /// Create a new data entry
        /// </summary>
        public TypeSafeDataEntry(string propertyName, object[] parameters)
        {
            PropertyName = propertyName;
            Parameters = parameters;
        }

        internal TypeSafeDataEntry(string propertyName, object[] parameters, bool overrideRestrictedNames = false,
            Type overrideType = null, string obsoleteWarning = null)
            : this(propertyName, parameters)
        {
            OverrideRestrictedNames = overrideRestrictedNames;
            OverrideType = overrideType;
            ObsoleteWarning = obsoleteWarning;
        }

        /// <summary>
        /// String used as property name of the generated entry. Will be filtered through reserved name
        /// and C# compliance checks
        /// </summary>
        public string PropertyName { get; private set; }

        /// <summary>
        /// The array of parameters passed to the data type constructor.
        /// If the data type is a primitive or string, this should have one entry
        /// of the exact same type.
        /// </summary>
        public object[] Parameters { get; private set; }

        internal bool OverrideRestrictedNames { get; private set; }
        internal Type OverrideType { get; private set; }
        internal string ObsoleteWarning { get; private set; }
    }
}
