using UnityEditor;
using UnityEngine;

namespace TypeSafe.Editor.Unity
{
    [CustomEditor(typeof (TypeCache))]
    internal class TypeCacheEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            TypeSafeGUI.DrawSettingsLogo();

            GUILayout.Label(Strings.CacheInspectorWarningText, EditorStyles.wordWrappedLabel);
            EditorGUILayout.Separator();

            base.OnInspectorGUI();
        }
    }
}