using System;
using System.Collections.Generic;
using TypeSafe.Editor.Unity;

namespace TypeSafe.Editor.DataSources
{
    internal class SortingLayerDataSource : ITypeSafeDataSource
    {
        public TypeSafeDataUnit GetTypeSafeDataUnit()
        {
            TSLog.Log(LogCategory.Scanner, "Beginning Sorting Layer Scan");

            var layerNames = InternalEditorUtilityWrapper.sortingLayerNames;
            var layerIds = InternalEditorUtilityWrapper.sortingLayerUniqueIDs;

            if (layerNames.Length != layerIds.Length)
            {
                throw new Exception("Sorting layer name and id array lengths do not match.");
            }

            var data = new List<TypeSafeDataEntry>();

            for (var i = 0; i < layerNames.Length; i++)
            {
                var name = layerNames[i];
                var id = layerIds[i];

                TSLog.Log(LogCategory.Scanner, string.Format("Sorting Layer {0}: name={1}, unique_id={2}", i, name, id));

                data.Add(new TypeSafeDataEntry(name, new object[] {name, id}));
            }

            TSLog.Log(LogCategory.Scanner, "Sorting Layer Scan Completed");

            return new TypeSafeDataUnit(TypeSafeUtil.GetFinalClassName(Strings.SortingLayersTypeName),
                typeof (SortingLayer), data, true, "SortingLayers");
        }
    }
}
