//#define DEMO
//#define DEV

namespace TypeSafe.Editor.Unity
{
    /// <summary>
    /// Class containing all interaction from the user to TypeSafe (menus etc)
    /// </summary>
    internal static class UserInteraction
    {

#if DEV
		[MenuItem("TypeSafe/(DEV) Clean")]
		public static void Clean()
		{
			TypeSafeUtil.Clean();
		}

		[MenuItem("TypeSafe/(DEV) Clear TypeSafe Editor Prefs")]
		public static void ClearEditorPrefs()
		{
			EditorPrefs.DeleteKey(Strings.EditorPrefs_BoolWelcomeWindowShown);
			EditorPrefs.DeleteKey(Strings.EditorPrefs_StringChangeLogVersion);
		}

	    [MenuItem("TypeSafe/(DEV) Open Changelog")]
	    public static void OpenChangelog()
	    {
	        WelcomeWindow.Open(true);
	    }

#endif

#if DEMO

		[MenuItem("TypeSafe/(DEMO) Fake Progress")]
		public static void FakeProgress()
		{
			SRProgressBar.Display("TypeSafe", "Scanning (60/253)", 0.2f);
		}

#endif
    }
}
