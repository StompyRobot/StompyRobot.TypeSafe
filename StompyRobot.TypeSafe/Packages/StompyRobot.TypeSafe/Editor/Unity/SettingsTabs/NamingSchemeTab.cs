using UnityEditor;
using UnityEngine;

namespace TypeSafe.Editor.Unity.SettingsTabs
{
    internal class NamingSchemeTab : ITab
    {
        private const float ErrorIconHeight = 16f;
        private Texture2D _errorIcon;
        private GUIStyle _errorLabelStyle;
        private string _newNamespace;
        private string _newPrefix;
        private string _newSuffix;

        public bool IsEnabled
        {
            get { return true; }
        }

        public string TabName
        {
            get { return "Name Scheme"; }
        }

        public bool CanExit
        {
            get { return true; }
        }

        public void OnEnter()
        {
            if (_errorIcon == null)
            {
                _errorIcon = EditorGUIUtility.FindTexture("d_console.erroricon.sml");
            }

            if (_errorLabelStyle == null)
            {
                _errorLabelStyle = new GUIStyle(EditorStyles.label);
                _errorLabelStyle.normal.textColor = Color.red;
            }

            if (_newNamespace == null)
            {
                _newNamespace = Settings.Instance.Namespace;
            }

            if (_newPrefix == null)
            {
                _newPrefix = Settings.Instance.ClassNamePrefix;
            }

            if (_newSuffix == null)
            {
                _newSuffix = Settings.Instance.ClassNameSuffix;
            }
        }

        public void OnExit()
        {
            if (HasChangedNaming() && ValidateNaming())
            {
                if (EditorUtility.DisplayDialog("TypeSafe - Apply Changes",
                    "Would you like to apply the changes you have made to the naming scheme?", "Yes", "Discard Changes"))
                {
                    Apply();
                }
            }
            else
            {
                Reset();
            }
        }

        public void OnGUI()
        {
            GUILayout.Label("Naming Scheme", EditorStyles.boldLabel);

            GUILayout.Label(
                "The naming scheme of the generated code can be adjusted here. Edit the namespace, prefix and suffix settings then press Apply to save and trigger a regeneration.",
                Styles.ParagraphLabel);

            var validationFailed = false;

            var width = (Screen.width - 8)/3;

            EditorGUILayout.BeginHorizontal(Styles.NamingPreviewHeaderGroupStyle);

            string errorMessage;

            EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
            GUILayout.Label("Namespace", Styles.NamingHeaderNamespaceLabel);

            if (!string.IsNullOrEmpty(_newNamespace) &&
                !TypeSafeUtil.ValidateNamespaceName(_newNamespace, out errorMessage))
            {
                GUILayout.Label(new GUIContent(_errorIcon, errorMessage), GUILayout.Height(ErrorIconHeight));
                validationFailed = true;
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal(GUILayout.Width(width));

            GUILayout.Label("Prefix", Styles.NamingHeaderPrefixLabel);

            if (!string.IsNullOrEmpty(_newPrefix) && !TypeSafeUtil.ValidateTypeName(_newPrefix, out errorMessage))
            {
                GUILayout.Label(new GUIContent(_errorIcon, errorMessage), GUILayout.Height(ErrorIconHeight));
                validationFailed = true;
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal(GUILayout.Width(width));

            GUILayout.Label("Suffix", Styles.NamingHeaderSuffixLabel);

            if (!string.IsNullOrEmpty(_newSuffix) && !TypeSafeUtil.ValidateTypeName(_newSuffix, out errorMessage))
            {
                GUILayout.Label(new GUIContent(_errorIcon, errorMessage), GUILayout.Height(ErrorIconHeight));
                validationFailed = true;
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal(Styles.NamingPreviewGroupStyle);

            _newNamespace = EditorGUILayout.TextField(_newNamespace, Styles.NamingHeaderTextBox);
            _newPrefix = EditorGUILayout.TextField(_newPrefix, Styles.NamingHeaderTextBox);
            _newSuffix = EditorGUILayout.TextField(_newSuffix, Styles.NamingHeaderTextBox);

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginVertical(Styles.NamingPreviewTable);

            var typeNames = TypeSafeUtil.GetTypeSafeClassNames();

            for (var i = 0; i < typeNames.Count; i++)
            {
                DrawNamingPreview(typeNames[i], i, ref validationFailed);
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(!HasChangedNaming() || validationFailed);

            if (GUILayout.Button("Apply"))
            {
                Apply();
            }

            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(!HasChangedNaming());

            if (GUILayout.Button("Reset", GUILayout.ExpandWidth(false)))
            {
                Reset();
            }

            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
        }

        private void Reset()
        {
            _newNamespace = Settings.Instance.Namespace;
            _newPrefix = Settings.Instance.ClassNamePrefix;
            _newSuffix = Settings.Instance.ClassNameSuffix;
        }

        private void DrawNamingPreview(string className, int index, ref bool validationFailed)
        {
            var style = index%2 == 0 ? Styles.NamingPreviewRowOdd : Styles.NamingPreviewRowEven;

            EditorGUILayout.BeginHorizontal(style);

            GUILayout.Label(_newNamespace, Styles.NamingPreviewNamespaceLabel);

            if (!string.IsNullOrEmpty(_newNamespace))
            {
                GUILayout.Label(".", Styles.NamingPreviewSeperatorLabel);
            }

            if (!string.IsNullOrEmpty(_newPrefix))
            {
                GUILayout.Label(_newPrefix, Styles.NamingPreviewPrefixLabel);
            }

            GUILayout.Label(className, Styles.NamingPreviewClassNameLabel);

            if (!string.IsNullOrEmpty(_newSuffix))
            {
                GUILayout.Label(_newSuffix, Styles.NamingPreviewSuffixLabel);
            }

            string errorMessage;

            if (!TypeSafeUtil.ValidateTypeName(className, _newNamespace, _newPrefix, _newSuffix, out errorMessage))
            {
                GUILayout.FlexibleSpace();

                GUILayout.Label(TypeSafeUtil.GetShortErrorMessage(errorMessage), _errorLabelStyle);
                GUILayout.Label(new GUIContent(_errorIcon, errorMessage), GUILayout.Height(ErrorIconHeight));
                validationFailed = true;
            }

            EditorGUILayout.EndHorizontal();
        }

        private bool HasChangedNaming()
        {
            if (_newNamespace != Settings.Instance.Namespace || _newPrefix != Settings.Instance.ClassNamePrefix ||
                _newSuffix != Settings.Instance.ClassNameSuffix)
            {
                return true;
            }

            return false;
        }

        private bool ValidateNaming()
        {
            var typeNames = TypeSafeUtil.GetTypeSafeClassNames();

            string errorMessage;

            for (var i = 0; i < typeNames.Count; i++)
            {
                if (
                    !TypeSafeUtil.ValidateTypeName(typeNames[i], _newNamespace, _newPrefix, _newSuffix, out errorMessage))
                {
                    return false;
                }
            }

            return true;
        }

        private void Apply()
        {
            Settings.Instance.Namespace = _newNamespace;
            Settings.Instance.ClassNamePrefix = _newPrefix;
            Settings.Instance.ClassNameSuffix = _newSuffix;
            Settings.Instance.Save();

            TypeSafeController.Instance.Refresh(true);
        }
    }
}
