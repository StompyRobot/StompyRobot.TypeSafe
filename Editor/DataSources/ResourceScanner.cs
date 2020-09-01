using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TypeSafe.Editor.Data;
using TypeSafe.Editor.Unity;
using UnityEditor;
using UnityEngine;

namespace TypeSafe.Editor.DataSources
{
    internal class ResourceScanner
    {
        public int TotalAssets { get; private set; }

        public IEnumerable<ResourceDefinition> Scan()
        {
            var paths =
                AssetDatabase.GetAllAssetPaths()
                             .Where(p => Path.HasExtension(p) && PathUtility.IsAssetResource(p))
                             .ToList();

            TotalAssets = paths.Count;

            foreach (var path in paths)
            {
                var name = Path.GetFileNameWithoutExtension(path);
                var relativePath = GetResourceRelativePath(path);

                var inBlacklist = PathUtility.IsInBlacklist(path);
                var inWhitelist = PathUtility.IsInWhitelist(path);

                var skip = inBlacklist || (!inWhitelist && Settings.Instance.EnableWhitelist);

                Type type = null;
                var source = UnityUtility.TypeSource.None;

                if (!skip)
                {
                    if (!UnityUtility.GetAssetType(path, out type, out source))
                    {
                        TSLog.LogError(LogCategory.Scanner, string.Format("Failed finding type for asset at path {0}", path));
                        continue;
                    }
                }

                var isEditorType = !skip && UnityUtility.IsEditorType(type);
                var isPrivate = !skip && (type.IsNotPublic && !UnityUtility.IsUserAssemblyType(type));

                TSLog.Log(LogCategory.Scanner,
	                string.Format("Resource [Name: {0}, RelativePath: {1}, FullPath: {2}, Type: {3}, isEditorType: {4}, typeSource: {5}, inBlacklist: {6}, inWhitelist: {7}, isPrivate: {8}]", name, relativePath, path, type != null ? type.FullName : null,
		                isEditorType, source, inBlacklist, inWhitelist, isPrivate));

                if (isPrivate)
                {
                    TSLog.Log(LogCategory.Scanner, string.Format("Skipping {0} (isPrivate=true)", relativePath));
                    continue;
                }

                // Skip resources that are of an editor-only type
                if (isEditorType)
                {
                    TSLog.Log(LogCategory.Scanner, string.Format("Skipping {0} (isEditorType=true)", relativePath));
                    continue;
                }

                // Skip resources that are of an editor-only type
                if (inBlacklist)
                {
                    TSLog.Log(LogCategory.Scanner, string.Format("Skipping {0} (inBlacklist=true)", relativePath));
                    continue;
                }

                // Skip resources that are not in the white list.
                if (Settings.Instance.EnableWhitelist && !inWhitelist)
                {
                    TSLog.Log(LogCategory.Scanner, string.Format("Skipping {0} (inWhitelist = false)", relativePath));
                    continue;
                }

                if (type == typeof(Texture2D))
                {
                    var importer = AssetImporter.GetAtPath(path) as TextureImporter;

                    if (importer != null && importer.textureType == TextureImporterType.Sprite)
                    {
                        TSLog.Log(LogCategory.Scanner, "Overriding type -> Sprite (importer.textureType == TextureImporterType.Sprite)");
                        type = typeof(Sprite);
                    }
                }

                var resourceDefinition = new ResourceDefinition(name,
                    relativePath, path, type);

                yield return resourceDefinition;
            }
        }

        private static string GetResourceRelativePath(string fullPath)
        {
            if (fullPath.Contains("\\"))
            {
                throw new FormatException(string.Format("Path must use forward-slash for path divider ({0})", fullPath));
            }

            var split = fullPath.Split('/');

            for (var i = 0; i < split.Length; i++)
            {
                if (split[i].ToLower() == "resources")
                {
                    split[split.Length - 1] = Path.GetFileNameWithoutExtension(split[split.Length - 1]);
                    return string.Join("/", split, i + 1, split.Length - i - 1);
                }
            }

            throw new Exception(string.Format("Path was not in a resources folder: {0}", fullPath));
        }
    }
}
