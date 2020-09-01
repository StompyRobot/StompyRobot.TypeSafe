using UnityEditor;
using UnityEngine;

namespace TypeSafe.Editor.Unity.WelcomeTabs
{
    internal class GuideTab : ITab
    {
        private Vector2 _scrollPosition;

        public bool IsEnabled
        {
            get { return true; }
        }

        public string TabName
        {
            get { return "Quick-Start Guide"; }
        }

        public bool CanExit
        {
            get { return true; }
        }

        public void OnEnter() {}
        public void OnExit() {}

        public void OnGUI()
        {
            //EditorGUILayout.BeginVertical(Styles.WelcomeTextBox);

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            GUILayout.BeginVertical();

            GUILayout.Label("Welcome", Styles.HeaderLabel);

            GUILayout.Label(
                "Thank you for downloading TypeSafe! By using TypeSafe throughout your project you will significantly improve the robustness of your codebase. Errors in your code introduced by changes via the Unity Editor will be a thing of the past.",
                Styles.ParagraphLabel);

            GUILayout.Label("Quick Start", Styles.HeaderLabel);

            GUILayout.Label(
                "TypeSafe allows you to replace any \"raw-strings\" in your code-base with strong-typed references to generated classes. " +
                "This way, if you change the location of a resource or modify layer names, you will discover errors that result from this instantly.",
                Styles.ParagraphLabel);

            GUILayout.Label(
                "TypeSafe watches for changes to your project and automatically regenerates the classes if changes occur.",
                Styles.ParagraphLabel);
            GUILayout.Label(
                "If you have just installed TypeSafe, you might want to trigger a scan manually from the <i>Assets/TypeSafe Refresh</i> menu item.",
                Styles.ParagraphLabel);

            if (!TypeSafeUtil.HasGeneratedBefore())
            {
                GUILayout.BeginHorizontal("CN EntryInfo");

                GUILayout.Label("Looks like you haven't run a scan yet, would you like to do it now?",
                    Styles.ParagraphLabel);

                GUI.enabled = TypeSafeController.Instance.State == States.Idle;

                if (GUILayout.Button("Start Scan", GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(true)))
                {
                    TypeSafeController.Instance.Queue(true);
                }

                GUI.enabled = true;

                GUILayout.EndHorizontal();
            }

            TypeSafeGUI.DrawScreenshot(Embedded.ResourcesDemo);

            GUILayout.Label(
                string.Format(
                    "Once the scan is complete, your resources will be available in the <b>{0}</b> class. You can use Intellisense to navigate this class from your code editor.",
                    TypeSafeUtil.GetFinalClassName(Strings.ResourcesTypeName)), Styles.ParagraphLabel);

            GUILayout.Label(
                string.Format(
                    "Similarly, your scenes (as listed in the build window), layers and tags will be available in <b>{0}</b>, <b>{1}</b> and <b>{2}</b> respectively.",
                    TypeSafeUtil.GetFinalClassName(Strings.ScenesTypeName),
                    TypeSafeUtil.GetFinalClassName(Strings.LayersTypeName),
                    TypeSafeUtil.GetFinalClassName(Strings.TagsTypeName)), Styles.ParagraphLabel);

            GUILayout.Label(
                "The naming scheme can be customized from the settings window. This can be accessed from <i>Window/TypeSafe/Open Settings</i> menu.",
                Styles.ParagraphLabel);

            TypeSafeGUI.DrawScreenshot(Embedded.UsageDemo);

            if (TypeSafeGUI.ClickableLabel(
	            string.Format("For more detailed information, <color={0}>click here</color> to visit the online documentation.", Strings.LinkColour),
                Styles.ParagraphLabel))
            {
                Application.OpenURL(Strings.DocumentationUrl);
            }

            GUILayout.Label("Thanks again for downloading, and we hope you find TypeSafe useful!", Styles.ParagraphLabel);

            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Version " + Strings.Version, EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();

            GUILayout.EndVertical();

            EditorGUILayout.EndScrollView();

            //EditorGUILayout.EndVertical();
        }
    }
}
