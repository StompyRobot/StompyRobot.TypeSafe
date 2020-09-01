using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace TypeSafe.Editor
{
    [Serializable]
    class TypeCacheEntry
    {
        public string Path;
        public string Type;
    }

    internal class TypeCache : ScriptableObject
    {
        private static TypeCache _instance;

        public TypeCacheEntry[] Contents;

        public static bool HasInstance
        {
            get
            {
                if (_instance != null)
                {
                    return true;
                }

                return (_instance = GetInstance(false)) != null;
            }
        }
        public static TypeCache Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = GetInstance(true);
                }

                return _instance;
            }
        }

        private static TypeCache GetInstance(bool autoCreate)
        {
            if (!TypeSafeUtil.IsEnabled())
            {
                throw new InvalidOperationException(
                    "Attempted to create settings instance while TypeSafe is disabled. Something should be checking TypeSafeUtil.IsEnabled() before running.");
            }

            var cachePath = PathUtility.GetTypeCachePath();

            TSLog.Log(LogCategory.Trace, string.Format("Checking for TypeCache asset at {0}", cachePath));

            var fileExists = File.Exists(cachePath);
            var instance = AssetDatabase.LoadAssetAtPath<TypeCache>(cachePath);
            
            if (instance == null && fileExists)
            {
                TSLog.Log(LogCategory.Trace, "TypeCache asset exists, but AssetDatabase doesn't know it. Likely project reimport in progress.");
                return null;
            }

            if (instance == null)
            {
                TSLog.Log(LogCategory.Trace, string.Format("TypeCache asset not found at {0} (fileExists={1}, instance=null)", cachePath, fileExists));

                if (!autoCreate)
                {
                    return null;
                }

                // Create instance
                instance = CreateInstance<TypeCache>();

                TSLog.Log(LogCategory.Info, string.Format("Creating TypeCache asset at {0}", cachePath));

                try
                {
                    string parentDirectory = Path.GetDirectoryName(cachePath);
                    if (parentDirectory != null && !Directory.Exists(parentDirectory))
                    {
                        Directory.CreateDirectory(parentDirectory);
                    }

                    AssetDatabase.CreateAsset(instance, cachePath);
                }
                catch (Exception e)
                {
                    TSLog.LogError(LogCategory.Info, "Error creating TypeCache asset.");
                    TSLog.LogError(LogCategory.Info, e.ToString());
                }
            }

            return instance;
        }

        public void Save()
        {
            EditorUtility.SetDirty(this);
        }
    }
}