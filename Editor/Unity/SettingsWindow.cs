using TypeSafe.Editor.Unity.SettingsTabs;
using UnityEditor;
using UnityEngine;

namespace TypeSafe.Editor.Unity
{
    internal class SettingsWindow : TabWindow
    {
        private const float ErrorIconHeight = 16f;

        public static void Open()
        {
            GetWindowWithRect<SettingsWindow>(new Rect(0, 0, 420, 560), true, Strings.SettingsWindowTitle, true);
        }

        protected override void OnRegisterTabs()
        {
            base.OnRegisterTabs();

            AddTab(new GeneralTab());
            AddTab(new NamingSchemeTab());
            AddTab(new AssetsTab());
            AddTab(new BlacklistTab());
            AddTab(new AdvancedTab());
        }

        protected override void OnGUI()
        {
            TypeSafeGUI.BeginDrawBackground();

            TypeSafeGUI.DrawSettingsLogo();

            TypeSafeGUI.EndDrawBackground();

            EditorGUILayout.BeginVertical(Styles.SettingsHeaderBoxStyle);
            EditorGUILayout.EndVertical();

            EditorGUI.BeginChangeCheck();

            base.OnGUI();

            if (EditorGUI.EndChangeCheck())
            {
                Settings.Instance.Save();
            }

            OnGUIFooter();
        }

        private void OnGUIFooter()
        {
            GUILayout.Label(Strings.SettingsRateBoxContents, EditorStyles.miniLabel);

            var width = position.width;
            var margin = (EditorStyles.miniButton.padding.left);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Github", GUILayout.Width(width/2f - margin)))
            {
                Application.OpenURL(Strings.GithubUrl);
            }

            if (GUILayout.Button("Documentation", GUILayout.Width(width/2f - margin)))
            {
                Application.OpenURL(Strings.DocumentationUrl);
            }

            EditorGUILayout.EndHorizontal();
        }
    }
}
