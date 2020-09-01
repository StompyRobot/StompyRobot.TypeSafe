using System;
using System.Collections.Generic;

namespace TypeSafe.Editor
{
    /// <summary>
    /// Class describing a data set used by TypeSafe for code generation.
    /// </summary>
    public class TypeSafeDataUnit
    {
        private string _fileName;
        private readonly ICollection<TypeSafeDataEntry> _data = new List<TypeSafeDataEntry>();
        private readonly ICollection<TypeSafeDataUnit> _nestedUnits = new List<TypeSafeDataUnit>();

        /// <summary>
        /// Create a new data unit
        /// </summary>
        /// <param name="className"><see cref="ClassName"/></param>
        /// <param name="dataType"><see cref="DataType"/></param>
        /// <param name="enableAllProperty"><see cref="EnableAllProperty"/></param>
        public TypeSafeDataUnit(string className, Type dataType, bool enableAllProperty = false)
        {
            if (className == null)
            {
                throw new ArgumentNullException("className");
            }

            if (dataType == null)
            {
                throw new ArgumentNullException("dataType");
            }

            ClassName = className;
            DataType = dataType;
            EnableAllProperty = enableAllProperty;
        }

        /// <summary>
        /// Create a new data unit
        /// </summary>
        /// <param name="className"><see cref="ClassName"/></param>
        /// <param name="dataType"><see cref="DataType"/></param>
        /// <param name="data"><see cref="Data"/></param>
        /// <param name="enableAllProperty"><see cref="EnableAllProperty"/></param>
        /// <param name="fileName"><see cref="FileName"/></param>
        public TypeSafeDataUnit(string className, Type dataType, IEnumerable<TypeSafeDataEntry> data,
            bool enableAllProperty = false, string fileName = null) : this(className, dataType, enableAllProperty)
        {
            foreach (var t in data)
            {
                Data.Add(t);
            }

            _fileName = fileName;
        }

        /// <summary>
        /// Class name that TypeSafe will use for your generated class.
        /// </summary>
        public string ClassName { get; private set; }

        /// <summary>
        /// The name of the file the generated class will be placed in. (Setting to YourName will result in
        /// YourName.Generated.cs).
        /// If null, ClassName will be used. Ignored if this is a nested data unit.
        /// </summary>
        public string FileName
        {
            get { return _fileName != null ? _fileName : ClassName; }
            set { _fileName = value; }
        }

        /// <summary>
        /// The Type of the data in your set. This should be a primitive, string or read-only class/struct in your own code.
        /// </summary>
        public Type DataType { get; private set; }

        /// <summary>
        /// The data used to populate your generated class
        /// </summary>
        public ICollection<TypeSafeDataEntry> Data
        {
	        get { return _data; }
        }

        /// <summary>
        /// Data units that will be nested in this unit.
        /// </summary>
        public ICollection<TypeSafeDataUnit> NestedUnits
        {
	        get { return _nestedUnits; }
        }

        /// <summary>
        /// If true, a property will be generated called "All" that will be a list of all the entries in your data set.
        /// </summary>
        public bool EnableAllProperty { get; set; }
    }
}
