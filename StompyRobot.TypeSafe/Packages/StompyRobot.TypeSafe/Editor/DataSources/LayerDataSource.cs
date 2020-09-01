using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;

namespace TypeSafe.Editor.DataSources
{
    internal class LayerDataSource : ITypeSafeDataSource
    {
        public TypeSafeDataUnit GetTypeSafeDataUnit()
        {
            var data = new List<TypeSafeDataEntry>();

            var layers = InternalEditorUtility.layers;

            foreach (var layer in layers)
            {
                var ignore = string.IsNullOrEmpty(layer) || layer.Trim().Length == 0;
                var num = LayerMask.NameToLayer(layer);

                TSLog.Log(LogCategory.Scanner, string.Format("Layer: {0}, (index: {1},ignore={2})", layer, num, ignore));

                if (!ignore)
                {
                    data.Add(new TypeSafeDataEntry(layer, new object[] {layer, num}));
                }
            }

            return new TypeSafeDataUnit(TypeSafeUtil.GetFinalClassName(Strings.LayersTypeName), typeof (Layer), data,
                true, "Layers");
        }
    }
}
