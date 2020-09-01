using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TypeSafe.Editor.Unity.SettingsTabs
{
    internal class AdvancedTab : ITab
    {
        private string _newPath;
        private IList<Type> _dataSources;
        private Vector2 _tabScrollPosition;

        public bool IsEnabled
        {
            get { return true; }
        }

        public string TabName
        {
            get { return "Advanced"; }
        }

        public bool CanExit
        {
            get { return true; }
        }

        public AdvancedTab()
        {
            _dataSources = TypeSafeUtil.GetCustomDataSourcesTypes().ToList();
        }

        public void OnEnter()
        {
            _newPath = Settings.Instance.OutputDirectory;
        }

        public void OnExit()
        {
            if (HasChangedPath())
            {
                if (EditorUtility.DisplayDialog("TypeSafe - Apply Changes",
                    "Would you like to apply the changes you have made to the output path?", "Yes", "Discard Changes"))
                {
                    Apply();
                }
            }
            else
            {
                _newPath = Settings.Instance.OutputDirectory;
            }
        }

        public void OnGUI()
        {
            _tabScrollPosition = EditorGUILayout.BeginScrollView(_tabScrollPosition);

            GUILayout.Label("Output Directory", EditorStyles.boldLabel);

            GUILayout.Label(
                "The output path is relative to your project assets directory (i.e. {PROJECT}/Assets).",
                Styles.ParagraphLabel);

            EditorGUILayout.BeginHorizontal();
            _newPath = EditorGUILayout.TextField(_newPath);

            EditorGUI.BeginDisabledGroup(_newPath == Strings.DefaultOutputPath);

            if (GUILayout.Button("Default", GUILayout.ExpandWidth(false)))
            {
                GUIUtility.keyboardControl = 0;
                _newPath = Strings.DefaultOutputPath;
            }

            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndHorizontal();

            GUILayout.Label("$TYPESAFE$ will be replaced by the path to the TypeSafe directory.",
                Styles.MiniLabelWrapped);

            EditorGUILayout.Space();

            string message;
            var isValid = ValidateNewPath(_newPath, out message);

            if (isValid)
            {
                EditorGUILayout.HelpBox(PathUtility.GetDeployDirectory(_newPath), MessageType.None, true);
            }
            else
            {
                EditorGUILayout.HelpBox(message, MessageType.Error, true);
            }

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(!isValid || !HasChangedPath());

            if (GUILayout.Button("Apply"))
            {
                Apply();
            }

            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(!HasChangedPath());

            if (GUILayout.Button("Reset", GUILayout.ExpandWidth(false)))
            {
                GUIUtility.keyboardControl = 0;
                _newPath = Settings.Instance.OutputDirectory;
            }

            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndHorizontal();

            GUILayout.Label(
                "TypeSafe will clear the current generated files and trigger a scan when changes to this path are applied.",
                Styles.MiniLabelWrapped);

            EditorGUILayout.Separator();

            GUILayout.Label("Disabled Scenes", EditorStyles.boldLabel);

            GUILayout.Label(
                "By default only the scenes in the Build Settings window that are 'active' are included in the generated scenes code. Enabling this option will include non-active scenes.",
                Styles.ParagraphLabel);

            Settings.Instance.IncludeNonActiveScenes = EditorGUILayout.Toggle("Include Disabled Scenes",
                Settings.Instance.IncludeNonActiveScenes);

            EditorGUILayout.HelpBox("Trying to load disabled scenes will fail at runtime. A compiler warning will occur wherever disabled scenes are used.", MessageType.Warning, true);

            EditorGUILayout.Space();

            GUILayout.Label("Toggle Data Sources", EditorStyles.boldLabel);
            GUILayout.Label(
                "Uncheck any data sources you wish to disable.",
                Styles.MiniLabelWrapped);

            EditorGUILayout.BeginVertical(Styles.ParagraphLabel);

            foreach (var dataSource in _dataSources)
            {
                var isEnabled = !Settings.Instance.DisabledDataSources.Contains(dataSource.AssemblyQualifiedName);

                if (EditorGUILayout.Toggle(
                    new GUIContent(dataSource.Name, dataSource.FullName),
                    isEnabled))
                {
                    // Detect toggle on
                    if (!isEnabled)
                    {
                        Settings.Instance.DisabledDataSources.Remove(dataSource.AssemblyQualifiedName);
                    }
                } else if (isEnabled)
                {
                    Settings.Instance.DisabledDataSources.Add(dataSource.AssemblyQualifiedName);
                }
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.EndScrollView();

            GUILayout.FlexibleSpace();
        }

        private bool HasChangedPath()
        {
            return _newPath != Settings.Instance.OutputDirectory;
        }

        private static bool ValidateNewPath(string path, out string message)
        {
            if (path.Length == 0)
            {
                message = "Path must not be empty";
                return false;
            }

            message = "";
            return true;
        }

        private void Apply()
        {
            TSLog.Log(LogCategory.Info, "Clearing current output directory.");
            TypeSafeUtil.Clean();

            Settings.Instance.OutputDirectory = _newPath;
            Settings.Instance.Save();

            TSLog.Log(LogCategory.Info, "Queuing scan.");
            TypeSafeApi.QueueRefresh();
        }
    }
}
