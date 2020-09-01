using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace TypeSafe.Editor.Unity.WelcomeTabs
{
    internal class ChangelogTab : ITab
    {
        private string _changelog;
        private Vector2 _scrollPosition;

        public bool IsEnabled
        {
            get { return true; }
        }

        public string TabName
        {
            get { return "Updates"; }
        }

        public bool CanExit
        {
            get { return true; }
        }

        public void OnEnter()
        {
            if (!string.IsNullOrEmpty(_changelog))
            {
                return;
            }

            var path = PathUtility.GetTypeSafePath();
            var filePath = Path.Combine(path, "CHANGELOG.txt");

            try
            {
                _changelog = File.ReadAllText(filePath);
            }
            catch (Exception e)
            {
                _changelog = "Error loading changelog.\n\n";
                _changelog += e.ToString();
            }
        }

        public void OnExit() {}

        public void OnGUI()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            GUILayout.BeginVertical();

            GUILayout.Label("Recent Updates", Styles.HeaderLabel);

            GUILayout.Label(_changelog, Styles.ParagraphLabel);

            GUILayout.EndVertical();

            EditorGUILayout.EndScrollView();
        }
    }
}
