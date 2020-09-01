using UnityEditor;
using UnityEngine;

namespace TypeSafe.Editor.Unity
{
    [CustomEditor(typeof (Settings))]
    internal class SettingsInspector : UnityEditor.Editor
    {
        private bool _override;

        public override void OnInspectorGUI()
        {
            TypeSafeGUI.DrawSettingsLogo();

            if (!_override)
            {
                GUILayout.Label(Strings.SettingsInspectorWarningText, EditorStyles.wordWrappedLabel);
                EditorGUILayout.Separator();
            }

            if (GUILayout.Button("Open Settings Window"))
            {
                TypeSafeApi.OpenSettingsWindow();
            }

            if (!_override)
            {
                if (GUILayout.Button("Override Warning"))
                {
                    _override = true;
                }
            }

            EditorGUILayout.Separator();

            if (_override)
            {
                GUILayout.Label("You have been warned...");
                base.OnInspectorGUI();
            }
        }
    }
}
