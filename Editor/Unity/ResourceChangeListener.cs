using System.Linq;
using UnityEditor;

namespace TypeSafe.Editor.Unity
{
    internal class TypeSafeAssetPostProcessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            // Short-circuit if all the paths are related to TypeSafe. Helps to reduce crashes when updating the DLLs
            if (importedAssets.All(PathUtility.IsTypeSafePath) && deletedAssets.All(PathUtility.IsTypeSafePath) &&
                movedAssets.All(PathUtility.IsTypeSafePath))
            {
                return;
            }

            TSLog.Log(LogCategory.Trace, "TypeSafeAssetPostProcessor.OnPostprocessAllAssets");

            if (!TypeSafeUtil.IsEnabled())
            {
                TSLog.Log(LogCategory.Trace, "OnPostProcessAllAssets, aborting. !IsEnabled()");
                return;
            }

            if (Settings.Instance == null)
            {
                TSLog.Log(LogCategory.Trace, "OnPostProcessAllAssets, aborting. Settings = null, likely AssetDatabase isn't ready yet.");
                return;
            }

            TSLog.Log(LogCategory.Trace, string.Format("Settings: AutoRebuild: {0}, ", Settings.Instance.AutoRebuild) +
                                         string.Format("TriggerOnResourceChange={0}, ", Settings.Instance.TriggerOnResourceChange) +
                                         string.Format("TriggerOnLayerChange={0}, ", Settings.Instance.TriggerOnLayerChange) +
                                         string.Format("TriggerOnInputChange={0}, ", Settings.Instance.TriggerOnInputChange) +
                                         string.Format("TriggerOnSceneChange={0}, ", Settings.Instance.TriggerOnSceneChange) +
                                         string.Format("TriggerOnAssetChange={0})", Settings.Instance.TriggerOnAssetChange));

            // Check for changes that require deleting entries in the asset type cache
            var changes = 0;

            for (var i = 0; i < importedAssets.Length; i++)
            {
                if (AssetTypeCache.ClearAssetType(importedAssets[i]))
                    changes++;
            }

            for (var i = 0; i < deletedAssets.Length; i++)
            {
                if (AssetTypeCache.ClearAssetType(deletedAssets[i]))
                    changes++;
            }

            for (var i = 0; i < movedFromAssetPaths.Length; i++)
            {
                if (AssetTypeCache.ClearAssetType(movedFromAssetPaths[i]))
                    changes++;
            }

            if (changes > 0)
            {
                TSLog.Log(LogCategory.Trace, string.Format("OnPostProcessAllAssets: Items removed from AssetTypeCache: {0}", changes));
                AssetTypeCache.SaveCache();
            }

            if (!Settings.Instance.AutoRebuild)
            {
                return;
            }

            if (deletedAssets.Length > 0)
            {
                if (TypeSafeUtil.ContainsTypeSafeTrackedAsset(deletedAssets) && Settings.Instance.TriggerOnAssetChange)
                {
                    TSLog.Log(LogCategory.Trace, "Queuing refresh due to asset removal.");
                    TypeSafeController.Instance.Refresh();
                    return;
                }
            }

            if (Settings.Instance.TriggerOnAssetChange && importedAssets.Select(AssetDatabase.AssetPathToGUID).Any(
                    p => Settings.Instance.AudioMixers.Contains(p) ||
                         Settings.Instance.Animators.Contains(p)))
            {
                TSLog.Log(LogCategory.Trace, "Queuing refresh due to asset change.");
                TypeSafeController.Instance.Refresh();
                return;
            }

            if (Settings.Instance.TriggerOnResourceChange &&
                importedAssets.Concat(deletedAssets)
                              .Concat(movedAssets)
                              .Concat(movedFromAssetPaths)
                              .Where(p => !PathUtility.IsInBlacklist(p) && (!Settings.Instance.EnableWhitelist || PathUtility.IsInWhitelist(p)))
                              .Any(PathUtility.IsAssetResource))
            {
                TSLog.Log(LogCategory.Trace, "Queuing refresh due to resources change.");
                TypeSafeController.Instance.Refresh();
                return;
            }

            var isSceneChange = importedAssets.Contains("ProjectSettings/EditorBuildSettings.asset");
            var isLayerChange = importedAssets.Contains("ProjectSettings/TagManager.asset");
            var isInputChange = importedAssets.Contains("ProjectSettings/InputManager.asset");

            TSLog.Log(LogCategory.Trace, string.Format("IsLayerChange: {0}, ", isLayerChange) +
                                         string.Format("IsSceneChange: {0}, ", isSceneChange) +
                                         string.Format("IsInputChange: {0}", isInputChange));

            if (isSceneChange && Settings.Instance.TriggerOnSceneChange)
            {
                TSLog.Log(LogCategory.Trace, "Queuing refresh due to scene change.");
                TypeSafeController.Instance.Refresh();
                return;
            }

            if (isLayerChange && Settings.Instance.TriggerOnLayerChange)
            {
                TSLog.Log(LogCategory.Trace, "Queuing refresh due to layer change.");
                TypeSafeController.Instance.Refresh();
                return;
            }

            if (isInputChange && Settings.Instance.TriggerOnInputChange)
            {
                TSLog.Log(LogCategory.Trace, "Queuing refresh due to input change.");
                TypeSafeController.Instance.Refresh();
            }
        }
    }
}
