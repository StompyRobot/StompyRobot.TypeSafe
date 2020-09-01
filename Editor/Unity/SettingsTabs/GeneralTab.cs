using UnityEditor;
using UnityEngine;

namespace TypeSafe.Editor.Unity.SettingsTabs
{
    internal class GeneralTab : ITab
    {
        public bool IsEnabled
        {
            get { return true; }
        }

        public string TabName
        {
            get { return "General"; }
        }

        public bool CanExit
        {
            get { return true; }
        }

        public void OnEnter() {}
        public void OnExit() {}

        public void OnGUI()
        {
            GUILayout.BeginHorizontal();

            GUILayout.Label(
                "You can trigger a manual scan from the <i>Assets/TypeSafe Refresh</i> menu item, or use this button.",
                Styles.ParagraphLabel);

            EditorGUI.BeginDisabledGroup(TypeSafeApi.IsBusy);

            if (GUILayout.Button("Start Scan", GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(true)))
            {
                TypeSafeController.Instance.Queue(true);
            }

            EditorGUI.EndDisabledGroup();

            GUILayout.EndHorizontal();

            GUILayout.Label("Automation", EditorStyles.boldLabel);

            GUILayout.Label(
                "TypeSafe can automatically trigger a scan when it detects changes to your project that will change the generated code.",
                Styles.ParagraphLabel);

            Settings.Instance.AutoRebuild = EditorGUILayout.Toggle(
                new GUIContent("Rebuild Automatically", "Scan for changes whenever you move a resource file."),
                Settings.Instance.AutoRebuild);

            EditorGUI.BeginDisabledGroup(!Settings.Instance.AutoRebuild);

            EditorGUILayout.Separator();

            GUILayout.Label("Minimum Build Time", EditorStyles.boldLabel);

            GUILayout.Label(
                "Force automatic builds to take a minimum amount of time to prevent interruptions while moving resources.",
                Styles.ParagraphLabel);

            EditorGUILayout.BeginHorizontal();

            Settings.Instance.EnableWaiting = EditorGUILayout.Toggle(
                new GUIContent("Automatic Rebuild Delay",
                    "Force automatic builds to take a minimum amount of time to prevent interruptions while moving resources."),
                Settings.Instance.EnableWaiting);

            if (Settings.Instance.EnableWaiting)
            {
                GUILayout.Label("(seconds): ");
                Settings.Instance.MinimumBuildTime = EditorGUILayout.Slider(Settings.Instance.MinimumBuildTime, 0.1f,
                    10f);
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Separator();

            GUILayout.Label("Triggers", EditorStyles.boldLabel);

            GUILayout.Label(
                "Uncheck any triggers that you do not wish to cause automatic scans.",
                Styles.ParagraphLabel);

            Settings.Instance.TriggerOnResourceChange = EditorGUILayout.Toggle(
                new GUIContent("Resources",
                    "Trigger an automatic rebuild when a relevant resource asset changes."),
                Settings.Instance.TriggerOnResourceChange);

            Settings.Instance.TriggerOnLayerChange = EditorGUILayout.Toggle(
                new GUIContent("Layers/Tags",
                    "Trigger an automatic rebuild when the layer/tag configuration changes."),
                Settings.Instance.TriggerOnLayerChange);

            Settings.Instance.TriggerOnSceneChange = EditorGUILayout.Toggle(
                new GUIContent("Scene",
                    "Trigger an automatic rebuild when the build scene list changes."),
                Settings.Instance.TriggerOnSceneChange);

            Settings.Instance.TriggerOnInputChange = EditorGUILayout.Toggle(
                new GUIContent("Input",
                    "Trigger an automatic rebuild when the input configuration changes."),
                Settings.Instance.TriggerOnInputChange);

            Settings.Instance.TriggerOnAssetChange = EditorGUILayout.Toggle(
                new GUIContent("Asset",
                    "Trigger an automatic rebuild when a tracked asset changes (see Assets tab)."),
                Settings.Instance.TriggerOnAssetChange);

            EditorGUI.EndDisabledGroup();

            GUILayout.FlexibleSpace();
        }
    }
}
