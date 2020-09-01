using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TypeSafe.Editor.Unity.SettingsTabs
{
    internal class BlacklistTab : ITab
    {
        private Vector2 _blacklistScrollPosition;
        private string _newPathBlacklist;
        private string _newPathWhitelist;
        private Vector2 _whitelistScrollPosition;

        public bool IsEnabled
        {
            get { return true; }
        }

        public string TabName
        {
            get { return "Filtering"; }
        }

        public bool CanExit
        {
            get { return true; }
        }

        public void OnEnter() {}
        public void OnExit() {}

        public void OnGUI()
        {
            GUILayout.Label("Blacklist", EditorStyles.boldLabel);

            GUILayout.Label(
                "Resources with a path that contains an entry in this list will be excluded.",
                Styles.ParagraphLabel);

            _blacklistScrollPosition = DrawList(Settings.Instance.ResourceFolderFilter, _blacklistScrollPosition,
                OnAddBlacklistItem, OnRemoveBlacklistItem);

            GUILayout.Label("Enter path here or drag-and-drop into the box above.", EditorStyles.miniLabel);

            EditorGUILayout.BeginHorizontal();

            _newPathBlacklist = EditorGUILayout.TextField(_newPathBlacklist, GUILayout.ExpandWidth(true));

            EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(_newPathBlacklist));

            if (GUILayout.Button("Add", EditorStyles.miniButton, GUILayout.ExpandWidth(false)))
            {
                GUIUtility.keyboardControl = 0;
                OnAddBlacklistItem(_newPathBlacklist);
                _newPathBlacklist = "";
            }

            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            Settings.Instance.EnableWhitelist = EditorGUILayout.Toggle(Settings.Instance.EnableWhitelist,
                GUILayout.Width(18));

            GUILayout.Label("Whitelist", EditorStyles.boldLabel);

            EditorGUILayout.EndHorizontal();

            EditorGUI.BeginDisabledGroup(!Settings.Instance.EnableWhitelist);

            GUILayout.Label(
                "Only resources that contain an entry in this list will be included in the generated code.",
                Styles.ParagraphLabel);

            _whitelistScrollPosition = DrawList(Settings.Instance.Whitelist, _whitelistScrollPosition,
                OnAddWhitelistItem, OnRemoveWhitelistItem);

            GUILayout.Label("Enter path here or drag-and-drop into the box above.", EditorStyles.miniLabel);

            EditorGUILayout.BeginHorizontal();

            _newPathWhitelist = EditorGUILayout.TextField(_newPathWhitelist, GUILayout.ExpandWidth(true));

            EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(_newPathWhitelist));

            if (GUILayout.Button("Add", EditorStyles.miniButton, GUILayout.ExpandWidth(false)))
            {
                GUIUtility.keyboardControl = 0;
                OnAddWhitelistItem(_newPathWhitelist);
                _newPathWhitelist = "";
            }

            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndHorizontal();

            EditorGUI.EndDisabledGroup();

            GUILayout.FlexibleSpace();

            /*
            var s = new GUIStyle();
            s.margin = new RectOffset(10, 10, 10, 10);

            GUILayout.BeginHorizontal(Styles.NamingPreviewTable);

            _folderScrollPosition = EditorGUILayout.BeginScrollView(_folderScrollPosition, Styles.NoPaddingNoMargin);

            EditorGUILayout.BeginVertical();

            for (var i = 0; i < Settings.Instance.ResourceFolderFilter.Length; i++) {

                EditorGUILayout.BeginHorizontal(Styles.NoPaddingNoMargin);

                Settings.Instance.ResourceFolderFilter[i] =
                    EditorGUILayout.TextField(Settings.Instance.ResourceFolderFilter[i]);

                if (GUILayout.Button(new GUIContent("", "Remove"), (GUIStyle)"OL Minus", GUILayout.ExpandWidth(false))) {

                    var l = Settings.Instance.ResourceFolderFilter.ToList();
                    l.RemoveAt(i);
                    Settings.Instance.ResourceFolderFilter = l.ToArray();
                    --i;

                }

                EditorGUILayout.EndHorizontal();

            }

            // Draw "New" row
            EditorGUILayout.BeginHorizontal(Styles.NoPaddingNoMargin);

            var newText = EditorGUILayout.TextField("");

            if (newText.Length > 0) {


                var l = Settings.Instance.ResourceFolderFilter.ToList();
                l.Add(newText);
                Settings.Instance.ResourceFolderFilter = l.ToArray();

            }

            GUILayout.Label("", Styles.NoPaddingNoMargin, GUILayout.Width(((GUIStyle)"OL Minus").CalcSize(new GUIContent("")).x));

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();

            GUILayout.EndHorizontal();*/
        }

        private static void OnAddBlacklistItem(string item)
        {
            Settings.Instance.ResourceFolderFilter.Add(item);
        }

        private static void OnRemoveBlacklistItem(int i)
        {
            Settings.Instance.ResourceFolderFilter.RemoveAt(i);
        }

        private static void OnAddWhitelistItem(string item)
        {
            Settings.Instance.Whitelist.Add(item);
        }

        private static void OnRemoveWhitelistItem(int i)
        {
            Settings.Instance.Whitelist.RemoveAt(i);
        }

        private Vector2 DrawList(IList<string> entries, Vector2 scrollPosition, Action<string> onAdd,
            Action<int> onRemove)
        {
            var rect = EditorGUILayout.BeginVertical(Styles.NamingPreviewTable, GUILayout.Height(85));

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, Styles.NoPaddingNoMargin);

            //EditorGUILayout.BeginVertical();

            for (var i = 0; i < entries.Count; i++)
            {
                EditorGUILayout.BeginHorizontal(i%2 == 0 ? Styles.NamingPreviewRowEven : Styles.NamingPreviewRowOdd);

                GUILayout.Label(entries[i], "OL Label");

                if (GUILayout.Button(new GUIContent("", "Remove"), "OL Minus", GUILayout.ExpandWidth(false)))
                {
                    onRemove(i);
                    break;
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

            var evt = Event.current;
            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!rect.Contains(evt.mousePosition))
                    {
                        break;
                    }

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();

                        foreach (var path in DragAndDrop.paths)
                        {
                            onAdd(path);
                        }
                    }

                    break;
            }

            return scrollPosition;
        }
    }
}
