using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;

namespace TypeSafe.Editor.DataSources
{
    internal class SceneDataSource : ITypeSafeDataSource
    {
        public TypeSafeDataUnit GetTypeSafeDataUnit()
        {
            var data = new List<TypeSafeDataEntry>();

            TSLog.Log(LogCategory.Scanner, "Beginning Scene Scan");

            var i = 0;

            var scenes = new List<string>();

            foreach (var scene in EditorBuildSettings.scenes)
            {
                if (!Settings.Instance.IncludeNonActiveScenes && !scene.enabled)
                {
                    continue;
                }

                if (scenes.Any(p => p == scene.path))
                {
                    TSLog.Log(LogCategory.Scanner, string.Format("Duplicate Scene: {0}, {1}", i, scene.path));
                    continue;
                }

                var name = Path.GetFileNameWithoutExtension(scene.path);

                TSLog.Log(LogCategory.Scanner, string.Format("Scene: {0} (name={1}, isEnabled: {2})", scene.path, name, scene.enabled));

                if (string.IsNullOrEmpty(name))
                {
                    TSLog.LogWarning(LogCategory.Scanner, "Scene name is empty, skipping.");
                    continue;
                }

                scenes.Add(scene.path);

                var warning = scene.enabled ? null : Strings.Warning_NonActiveScene;

                data.Add(new TypeSafeDataEntry(name, new object[] {name, scene.enabled ? i : -1},
                    obsoleteWarning: warning));

                if (scene.enabled)
                {
                    i++;
                }
            }

            TSLog.Log(LogCategory.Scanner, "Scene Scan Complete");

            return new TypeSafeDataUnit(TypeSafeUtil.GetFinalClassName(Strings.ScenesTypeName), typeof (Scene), data,
                true,
                "Scenes");
        }
    }
}
