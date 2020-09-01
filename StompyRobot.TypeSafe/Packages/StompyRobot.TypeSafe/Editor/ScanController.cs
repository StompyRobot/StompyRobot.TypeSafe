using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using TypeSafe.Editor.Data;
using TypeSafe.Editor.DataSources;
using TypeSafe.Editor.Unity;
using UnityEngine;

namespace TypeSafe.Editor
{
    internal class ScanResult
    {
        public ScanResult(ResourceDatabase db, IEnumerable<TypeSafeDataUnit> dataUnits)
        {
            ResourceDatabase = db;
            DataUnits = dataUnits;
        }

        public ResourceDatabase ResourceDatabase { get; private set; }
        public IEnumerable<TypeSafeDataUnit> DataUnits { get; private set; }
    }

    internal class ScanController
    {
        private IEnumerator _scanProgressEnumerator;

        /// <summary>
        /// Max time the scanner can take per update
        /// </summary>
        public float MaxUpdateProcessingTime = 0.032f;

        /// <summary>
        /// Number of items in the current operation completed
        /// </summary>
        public int ItemsCompleted { get; private set; }

        /// <summary>
        /// Total number of items to process in the current operation
        /// </summary>
        public int TotalItems { get; private set; }

        public bool IsDone { get; private set; }
        public ScanResult Result { get; private set; }
        public bool WasSuccessful { get; private set; }

        public void Begin()
        {
            if (_scanProgressEnumerator != null)
            {
                throw new InvalidOperationException("Scan already in progress");
            }

            TSLog.Log(LogCategory.Scanner, "ScanController.Begin");

            IsDone = false;
            WasSuccessful = false;

            _scanProgressEnumerator = ScanProcess().GetEnumerator();
        }

        public void Update()
        {
            if (IsDone)
            {
                return;
            }

            try
            {
                _scanProgressEnumerator.MoveNext();
            }
            catch (Exception e)
            {
                TSLog.LogError(LogCategory.Info, "Exception occured while scanning.");
                TSLog.LogError(LogCategory.Info, e.ToString());

                IsDone = true;
                WasSuccessful = false;
            }
        }

        private IEnumerable ScanProcess()
        {
            TSLog.Log(LogCategory.Scanner, "ScanController.ScanProcess start");

            ResourceDatabase db;

            var sw = new Stopwatch();
            sw.Start();

            using (var scanIterator = ResourceScanProcess().GetEnumerator())
            {
                while (scanIterator.MoveNext())
                {
                    if (sw.Elapsed.TotalSeconds > MaxUpdateProcessingTime)
                    {
                        TSLog.Log(LogCategory.Scanner, "ScanProcess Suspending");

                        Resources.UnloadUnusedAssets();

                        sw.Stop();
                        sw.Reset();

                        yield return null;

                        sw.Start();
                    }
                }

                db = scanIterator.Current;
            }

            var dataUnits = new List<TypeSafeDataUnit>();

            TSLog.Log(LogCategory.Scanner, "Beginning ITypeSafeDataSource hooks");

            foreach (var dataSource in TypeSafeUtil.GetCustomDataSources())
            {
                if (Settings.Instance.DisabledDataSources.Contains(dataSource.GetType().AssemblyQualifiedName))
                {
                    TSLog.Log(LogCategory.Scanner, string.Format("Skipping {0}", dataSource.GetType().FullName));
                    continue;
                }

                TSLog.Log(LogCategory.Scanner, string.Format("Processing {0}", dataSource.GetType().FullName));

                try
                {
                    var data = dataSource.GetTypeSafeDataUnit();

                    if (ValidateDataUnit(data))
                    {
                        dataUnits.Add(data);
                    }
                }
                catch (Exception e)
                {
                    TSLog.LogError(LogCategory.Info,
	                    string.Format("Exception occured inside data source ({0})", dataSource.GetType().FullName));
                    TSLog.LogError(LogCategory.Info, e.ToString());
                }
            }

            TSLog.Log(LogCategory.Scanner, "Done running ITypeSafeDataSource hooks");

            Result = new ScanResult(db, dataUnits);

            TSLog.Log(LogCategory.Scanner, "ScanProcess Done");

            IsDone = true;
            WasSuccessful = true;
        }

        private IEnumerable<ResourceDatabase> ResourceScanProcess()
        {
            var resourceDatabase = new ResourceDatabase();

            var scanner = new ResourceScanner();

            var i = 0;

            foreach (var resourceDefinition in scanner.Scan())
            {
                resourceDatabase.Add(resourceDefinition);

                TotalItems = scanner.TotalAssets;
                ItemsCompleted = i;

                ++i;
                yield return null;
            }

            resourceDatabase.Validate();
            AssetTypeCache.SaveCache();

            yield return resourceDatabase;
        }

        private bool ValidateDataUnit(TypeSafeDataUnit unit, bool isNested = false)
        {
            if (string.IsNullOrEmpty(unit.ClassName) || string.IsNullOrEmpty(unit.ClassName.Trim()))
            {
                TSLog.LogError(LogCategory.Compile, string.Format("Error validating TypeSafeDataUnit ({0})", unit.ClassName));
                TSLog.LogError(LogCategory.Compile, Strings.Error_NameMustNotBeEmpty);
                return false;
            }

            string errorMessage;

            if (!isNested && !TypeSafeUtil.ValidateTypeName(unit.ClassName, out errorMessage))
            {
                TSLog.LogError(LogCategory.Compile, string.Format("Error validating TypeSafeDataUnit ({0})", unit.ClassName));
                TSLog.LogError(LogCategory.Compile, errorMessage);
                return false;
            }

            if (!isNested && !TypeSafeUtil.TestTypeNameConflicts(unit.ClassName))
            {
                TSLog.LogError(LogCategory.Compile, string.Format("Error validating TypeSafeDataUnit ({0})", unit.ClassName));
                TSLog.LogError(LogCategory.Compile, Strings.Error_ConflictsWithExistingType);
                return false;
            }

            foreach (var nestedUnit in unit.NestedUnits)
            {
                if (!ValidateDataUnit(nestedUnit, true))
                {
                    return false;
                }
            }

            foreach (var data in unit.Data)
            {
                if (string.IsNullOrEmpty(data.PropertyName.Trim()))
                {
                    TSLog.LogError(LogCategory.Compile, "Error validating data entry");
                    TSLog.LogError(LogCategory.Compile, Strings.Error_NameMustNotBeEmpty);
                    return false;
                }

                var dataType = /*data.OverrideDataType ??*/ unit.DataType;

                if ((dataType.IsPrimitive || dataType == typeof (string)) && data.Parameters.Length == 1 &&
                    data.Parameters[0] != null && data.Parameters[0].GetType() == dataType)
                {
                    continue;
                }

                foreach (var parameter in data.Parameters)
                {
                    if (parameter == null)
                    {
                        TSLog.LogError(LogCategory.Compile, "Data parameter must not be null");
                        return false;
                    }

                    var pType = parameter.GetType();

                    if (pType.IsPrimitive)
                    {
                        continue;
                    }

                    if (pType == typeof (string))
                    {
                        continue;
                    }

                    TSLog.LogError(LogCategory.Compile, "Data constructor parameter must be primitive or string");

                    return false;
                }

                var pTypeArray = data.Parameters.Select(p => p.GetType()).ToArray();

                var constructor = dataType.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, pTypeArray,
                    null);

                if (constructor == null)
                {
                    var sig = string.Join(", ", pTypeArray.Select(p => p.FullName).ToArray());
                    TSLog.LogError(LogCategory.Compile,
	                    string.Format("Data Type does not have constructor that matches provided data ({0})", sig));
                }
            }

            return true;
        }
    }
}
