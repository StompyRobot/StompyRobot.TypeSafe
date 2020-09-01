#if TYPESAFE_ENABLE_ADDRESSABLES
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TypeSafe.Editor.Unity;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine.AddressableAssets;

namespace TypeSafe.Editor.DataSources
{
    internal class AddressablesDataSource : ITypeSafeDataSource
    {
        private static readonly char[] PathSeparator = { '/' };

        private static readonly Type AssetReferenceType = typeof(AssetReference);
        private static readonly Type AssetReferenceTType = typeof(AssetReferenceT<>);

        public TypeSafeDataUnit GetTypeSafeDataUnit()
        {
            var dataUnit = new TypeSafeDataUnit("Addressables", AssetReferenceType);
            if (!AddressableAssetSettingsDefaultObject.SettingsExists)
            {
                return dataUnit;
            }

            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            var addressableAssetEntries = new List<AddressableAssetEntry>();
            settings.GetAllAssets(addressableAssetEntries);
            
            foreach (AddressableAssetEntry entry in addressableAssetEntries)
            {
                (IEnumerable<string> path, TypeSafeDataEntry dataEntry) = ProcessAsset(entry);

                var currentUnit = dataUnit;
                foreach (string s in path)
                {
                    var nestedUnit = currentUnit.NestedUnits.FirstOrDefault(p => p.ClassName == s);
                    if (nestedUnit == null)
                    {
                        nestedUnit = new TypeSafeDataUnit(s, AssetReferenceType);
                        currentUnit.NestedUnits.Add(nestedUnit);
                    }

                    currentUnit = nestedUnit;
                }

                currentUnit.Data.Add(dataEntry);
            }

            return dataUnit;
        }

        private (IEnumerable<string> path, TypeSafeDataEntry) ProcessAsset(AddressableAssetEntry entry)
        {
            Type entryType;

            Type assetType;
            UnityUtility.TypeSource source;

            if (UnityUtility.GetAssetType(entry.AssetPath, out assetType, out source))
            {
                entryType = AssetReferenceTType.MakeGenericType(assetType);
            }
            else
            {
                TSLog.LogWarning(LogCategory.Scanner, string.Format("Failed finding type for asset at path {0}", entry.AssetPath));
                entryType = AssetReferenceType;
            }

            string[] path = entry.address.Split(PathSeparator, StringSplitOptions.RemoveEmptyEntries);

            return (path.Take(path.Length-1), new TypeSafeDataEntry(path.Last(), new object[] {entry.guid}, overrideType: entryType));
        }
    }
}
#endif