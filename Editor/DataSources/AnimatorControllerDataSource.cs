using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;

namespace TypeSafe.Editor.DataSources
{
    internal class AnimatorControllerDataSource : ITypeSafeDataSource
    {
        public TypeSafeDataUnit GetTypeSafeDataUnit()
        {
            var rootUnit = new TypeSafeDataUnit(TypeSafeUtil.GetFinalClassName(Strings.AnimatorsName)
                , typeof(float), new List<TypeSafeDataEntry>(), false, "Animators");

            var animators = Settings.Instance.Animators;

            foreach (var guid in animators)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<AnimatorController>(assetPath);

                TSLog.Log(LogCategory.Trace, string.Format("AnimatorController: {0}", assetPath), asset);
                rootUnit.NestedUnits.Add(CreateAnimatorDataUnit(asset));
            }

            return rootUnit;
        }

        private TypeSafeDataUnit CreateAnimatorDataUnit(AnimatorController controller)
        {
            var unit = new TypeSafeDataUnit(controller.name, typeof(string));
            
            var parametersUnit = new TypeSafeDataUnit("Parameters", typeof(int));
            foreach (var param in controller.parameters)
            {
                TSLog.Log(LogCategory.Trace, string.Format("Parameter: {0} ({1})", param.name, param.nameHash));

                if (parametersUnit.Data.Any(p => p.PropertyName == param.name))
                {
                    TSLog.LogWarning(LogCategory.Compile, string.Format("[AnimatorController] Duplicate parameter name ({0}).", param.name), controller);
                    continue;
                }

                parametersUnit.Data.Add(new TypeSafeDataEntry(param.name, new object[] { param.nameHash }));
            }

            var layersUnit = new TypeSafeDataUnit("Layers", typeof(int));
            for (var i = 0; i < controller.layers.Length; i++)
            {
                var layer = controller.layers[i];
                TSLog.Log(LogCategory.Trace, string.Format("Layer: {0}, Index: {1}", layer.name, i));

                if (layersUnit.Data.Any(p => p.PropertyName == layer.name))
                {
                    TSLog.LogWarning(LogCategory.Compile, string.Format("[AnimatorController] Duplicate layer name ({0}).", layer.name),
                        controller);
                    continue;
                }

                layersUnit.Data.Add(new TypeSafeDataEntry(layer.name, new object[] {i}));
            }

            unit.NestedUnits.Add(parametersUnit);
            unit.NestedUnits.Add(layersUnit);

            return unit;
        }
    }
}