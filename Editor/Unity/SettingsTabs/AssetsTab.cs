using TypeSafe.Editor.Unity.UI;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Audio;

namespace TypeSafe.Editor.Unity.SettingsTabs
{
    internal class AssetsTab : ITab
    {
        private readonly AssetTable _mixerTable = new AssetTable(new[] {typeof(AudioMixer)});
        private readonly AssetTable _animatorTable = new AssetTable(new[] {typeof(AnimatorController)});

        public bool IsEnabled
        {
	        get { return true; }
        }

        public string TabName
        {
	        get { return "Assets"; }
        }

        public bool CanExit
        {
	        get { return true; }
        }

        public AssetsTab()
        {
            _mixerTable.Added = OnAudioMixerAdd;
            _mixerTable.Removed = OnAudioMixerRemove;
            _animatorTable.Added = OnAnimatorAdded;
            _animatorTable.Removed = OnAnimatorRemoved;
        }

        public void OnEnter() { }
        public void OnExit() { }

        public void OnGUI()
        {
            GUILayout.Label("Audio Mixers", EditorStyles.boldLabel);

            GUILayout.Label(
	            string.Format("Audio mixers in this list will be added to the {0} class.", TypeSafeUtil.GetFinalClassName(Strings.AudioMixersName)),
                Styles.ParagraphLabel);

            _mixerTable.Draw(Settings.Instance.AudioMixers);

            GUILayout.Label("Drag-and-drop an audio mixer asset into the box above.", EditorStyles.miniLabel);

            EditorGUILayout.Space();

            GUILayout.Label("Animators", EditorStyles.boldLabel);

            GUILayout.Label(
	            string.Format("Animators in this list will be added to the {0} class.", TypeSafeUtil.GetFinalClassName(Strings.AnimatorsName)),
                Styles.ParagraphLabel);

            _animatorTable.Draw(Settings.Instance.Animators);

            GUILayout.Label("Drag-and-drop an animator asset into the box above.", EditorStyles.miniLabel);
            
            GUILayout.FlexibleSpace();
        }

        private static void OnAudioMixerAdd(string itemGuid)
        {
            if (Settings.Instance.AudioMixers.Contains(itemGuid))
            {
                return;
            }

            Settings.Instance.AudioMixers.Add(itemGuid);
        }

        private static void OnAudioMixerRemove(string itemGuid)
        {
            Settings.Instance.AudioMixers.Remove(itemGuid);
        }

        private static void OnAnimatorAdded(string itemGuid)
        {
            if (Settings.Instance.Animators.Contains(itemGuid))
            {
                return;
            }

            Settings.Instance.Animators.Add(itemGuid);
        }

        private static void OnAnimatorRemoved(string itemGuid)
        {
            Settings.Instance.Animators.Remove(itemGuid);
        }
    }
}
