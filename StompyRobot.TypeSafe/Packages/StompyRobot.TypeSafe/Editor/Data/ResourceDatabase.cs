using System;
using System.Linq;
using TypeSafe.Editor.Compiler;
using UnityEditor;
using Object = UnityEngine.Object;

namespace TypeSafe.Editor.Data
{
    internal class ResourceDatabase
    {
	    private readonly ResourceFolder _rootFolder = new ResourceFolder("Root", "");

	    public ResourceFolder RootFolder
	    {
		    get { return _rootFolder; }
	    }

	    public void Add(ResourceDefinition definition)
        {
            var folder = GetFolderForPath(definition.Path);

            if (definition.Name == folder.Name)
            {
                TSLog.LogWarning(LogCategory.Compile, Strings.Warning_ResourceNameCannotBeSameAsParent,
                    AssetDatabase.LoadAssetAtPath(definition.FullPath, typeof (Object)));

                definition = new ResourceDefinition("_" + definition.Name, definition.Path, definition.FullPath,
                    definition.Type);
            }

            foreach (var p in folder.Resources)
            {
                if (p.Name == definition.Name)
                {
                    TSLog.LogError(LogCategory.Compile,
	                    string.Format("Name for resource at [{0}] conflicts with another resource.", definition.FullPath),
                        AssetDatabase.LoadAssetAtPath(definition.FullPath, typeof (Object)));

                    TSLog.LogError(LogCategory.Compile, string.Format("  \\_ Other Resource: {0}", p.FullPath),
                        AssetDatabase.LoadAssetAtPath(p.FullPath, typeof (Object)));

                    return;
                }
            }

            folder.Resources.Add(definition);
        }

        public bool Remove(ResourceDefinition definition)
        {
            var folder = GetFolderForPath(definition.Path);

            var existing = folder.Resources.FirstOrDefault(p => p.FullPath == definition.FullPath);

            if (existing != null)
            {
                return folder.Resources.Remove(existing);
            }

            return false;
        }

        /// <summary>
        /// Perform validation on the database, ensuring there are no duplicates or resources with the same name as folders.
        /// </summary>
        public void Validate()
        {
            ValidateFolder(RootFolder);
        }

        private void ValidateFolder(ResourceFolder folder)
        {
            // Search for resources with the same name as a folder in the same directory
            for (var i = 0; i < folder.Resources.Count; i++)
            {
                var r = folder.Resources[i];

                if (folder.Folders.Any(p => p.Name == r.Name))
                {
                    TSLog.LogWarning(LogCategory.Compile,
	                    string.Format("Name for resource at [{0}] conflicts with a folder of the same name.", r.FullPath),
                        AssetDatabase.LoadAssetAtPath(r.FullPath, typeof (Object)));

                    // Remove existing resource
                    if (!Remove(r))
                    {
                        throw new Exception("Fatal error, failed removing resource from ResourceDatabase. ");
                    }

                    // Add again with new name
                    folder.Resources.Add(new ResourceDefinition("_" + r.Name, r.Path, r.FullPath, r.Type));
                }
            }

            foreach (var resourceFolder in folder.Folders)
            {
                ValidateFolder(resourceFolder);
            }
        }

        private ResourceFolder GetFolderForPath(string path)
        {
            var components = path.Split('/');

            var f = RootFolder;

            for (var i = 0; i < components.Length - 1; i++)
            {
                var c = CompilerUtil.GetSafeName(components[i], true);

                var isDuplicate = f.Name == c;
                var folderName = isDuplicate ? "_" + c : c;

                var folder = f.Folders.FirstOrDefault(p => p.Name == folderName);

                if (folder == null)
                {
                    var folderPath = components.Take(i).ToArray();

                    // Post warning the first time this folder is encountered
                    if (isDuplicate)
                    {
                        TSLog.LogWarning(LogCategory.Compile,
                            string.Format(Strings.Warning_FolderNameCannotBeSameAsParent, string.Join("/", folderPath)));
                    }

                    folder = new ResourceFolder(folderName, string.Join("/", folderPath));
                    f.Folders.Add(folder);
                }

                f = folder;
            }

            return f;
        }
    }
}
