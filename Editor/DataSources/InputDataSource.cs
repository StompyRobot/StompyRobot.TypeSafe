using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace TypeSafe.Editor.DataSources
{
    internal class InputDataSource : ITypeSafeDataSource
    {
        public TypeSafeDataUnit GetTypeSafeDataUnit()
        {
            var data = new List<TypeSafeDataEntry>();

            var inputManager =
                new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0]);

            var axesArray = inputManager.FindProperty("m_Axes");

            TSLog.Log(LogCategory.Scanner, string.Format("Axes Array: {0}, count: {1}", axesArray, axesArray.arraySize));

            for (var i = 0; i < axesArray.arraySize; i++)
            {
                var entry = axesArray.GetArrayElementAtIndex(i);

                var name = entry.FindPropertyRelative("m_Name").stringValue;

                var hasPositive = !string.IsNullOrEmpty(entry.FindPropertyRelative("positiveButton").stringValue) ||
                                  !string.IsNullOrEmpty(entry.FindPropertyRelative("altPositiveButton").stringValue);

                var hasNegative = !string.IsNullOrEmpty(entry.FindPropertyRelative("negativeButton").stringValue) ||
                                  !string.IsNullOrEmpty(entry.FindPropertyRelative("altNegativeButton").stringValue);

                var isButton = (hasPositive && !hasNegative) || (!hasPositive && hasNegative);

                TSLog.Log(LogCategory.Scanner,
	                string.Format("Input Axis: {0}, hasPositive: {1}, hasNegative: {2}, isButton: {3}", name, hasPositive, hasNegative, isButton));

                if (!data.Any(p => p.PropertyName == name))
                {
                    data.Add(new TypeSafeDataEntry(name, new object[] {name}));
                }
            }

            return new TypeSafeDataUnit(TypeSafeUtil.GetFinalClassName(Strings.InputTypeName), typeof (InputAxis), data,
                false, "Input");
        }
    }
}
