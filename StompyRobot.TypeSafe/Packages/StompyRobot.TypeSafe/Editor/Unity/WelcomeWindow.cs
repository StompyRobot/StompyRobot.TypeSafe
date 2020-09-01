using TypeSafe.Editor.Unity.WelcomeTabs;
using UnityEditor;
using UnityEngine;

namespace TypeSafe.Editor.Unity
{
    internal class WelcomeWindow : TabWindow
    {
        private static bool _openChangelog;
        private static bool? _hasShownWelcome;

        public static void Open(bool changelog = false)
        {
            _openChangelog = changelog;

            GetWindowWithRect<WelcomeWindow>(new Rect(Screen.width - 300f, Screen.height - 300f, 400f, 550f), true,
                "TypeSafe", true);

            _hasShownWelcome = true;
            EditorPrefs.SetBool(Strings.EditorPrefs_BoolWelcomeWindowShown, true);
            EditorPrefs.SetString(Strings.EditorPrefs_StringChangeLogVersion, Strings.Version);
            Settings.Instance.HasShownWelcome = true;
            Settings.Instance.Save();
        }

        public static bool HasOpenedBefore()
        {
            if (!_hasShownWelcome.HasValue)
            {
                _hasShownWelcome = EditorPrefs.GetBool(Strings.EditorPrefs_BoolWelcomeWindowShown, false);
            }

            return _hasShownWelcome.Value;
        }

        public static string GetPreviousOpenedVersion()
        {
            if (!EditorPrefs.HasKey(Strings.EditorPrefs_StringChangeLogVersion))
            {
                return null;
            }

            return EditorPrefs.GetString(Strings.EditorPrefs_StringChangeLogVersion);
        }

        protected override void OnRegisterTabs()
        {
            base.OnRegisterTabs();

            AddTab(new GuideTab());
            AddTab(new ChangelogTab());

            if (_openChangelog)
            {
                SetActiveTab(1);
                _openChangelog = false;
            }
        }

        protected override void OnGUI()
        {
            BoxStyleOverride = Styles.WelcomeTextBox;

            TypeSafeGUI.BeginDrawBackground();
            TypeSafeGUI.DrawWelcomeLogo();
            TypeSafeGUI.EndDrawBackground();

            base.OnGUI();
        }
    }
}
