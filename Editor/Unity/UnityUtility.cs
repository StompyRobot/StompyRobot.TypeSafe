using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

namespace TypeSafe.Editor.Unity
{
    internal static class UnityUtility
    {
        public enum TypeSource
        {
            None,
            FileName,
            Cache,
            Unity
        }

        private static readonly Dictionary<string, Type> _knownTypes = new Dictionary<string, Type>
        {
            {".prefab", typeof(GameObject)},
            {".mat", typeof(Material)},
            {".anim", typeof(AnimationClip)},
            {".shader", typeof(Shader)},
            {".cubemap", typeof(Cubemap)},

            {".psd", typeof(Texture2D)},
            {".jpg", typeof(Texture2D)},
            {".png", typeof(Texture2D)},
            {".jpeg", typeof(Texture2D)},
            {".gif", typeof(Texture2D)},
            {".bmp", typeof(Texture2D)},
            {".tga", typeof(Texture2D)},
            {".tiff", typeof(Texture2D)},
            {".tif", typeof(Texture2D)},
            {".iff", typeof(Texture2D)},
            {".pict", typeof(Texture2D)},
            {".dds", typeof(Texture2D)},

            {".mp3", typeof(AudioClip)},
            {".ogg", typeof(AudioClip)},
            {".aiff", typeof(AudioClip)},
            {".wav", typeof(AudioClip)},
            {".mod", typeof(AudioClip)},
            {".it", typeof(AudioClip)},
            {".sm3", typeof(AudioClip)},

            {".fbx", typeof(Mesh)},
            {".obj", typeof(Mesh)},
            {".blend", typeof(Mesh)},
            {".dae", typeof(Mesh)},
            {".3ds", typeof(Mesh)},
            {".dxf", typeof(Mesh)},
            {".max", typeof(Mesh)},

            {".mixer", typeof(AudioMixer)},
            {".controller", typeof(RuntimeAnimatorController)}
        };

        public static bool GetAssetType(string assetPath, out Type type, out TypeSource source)
        {
            var ext = Path.GetExtension(assetPath);

            if (!string.IsNullOrEmpty(ext))
            {
                if (_knownTypes.TryGetValue(ext.ToLower(), out type))
                {
                    source = TypeSource.FileName;
                    return true;
                }
            }

            if (AssetTypeCache.GetAssetType(assetPath, out type))
            {
                source = TypeSource.Cache;
                return true;
            }

            source = TypeSource.Unity;
            var asset = AssetDatabase.LoadMainAssetAtPath(assetPath);

            if (asset == null)
            {
                TSLog.LogWarning(LogCategory.Trace, string.Format("Failed loading asset at path [{0}].", assetPath));
                type = null;
                return false;
            }

            type = asset.GetType();
            AssetTypeCache.PostAssetType(assetPath, type);

            return true;
        }

        /// <summary>
        /// Checks if a Type is defined in one of the Unity-generated user assemblies.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static bool IsUserAssemblyType(Type t)
        {
            // Check if assembly is referencing the UnityEditor dll, and is not a Unity-generated assembly itself
            if (t.Assembly.FullName.Contains("Assembly-") &&
                t.Assembly.GetReferencedAssemblies().Any(p => p.Name == "UnityEngine"))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Check if type is defined in an editor assembly
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static bool IsEditorType(Type t)
        {
            // Early out if type is contained in UnityEditor
            if (t.Assembly.FullName.Contains("UnityEditor"))
            {
                return true;
            }

            // Early out if contained in a unity-generated editor dll
            if (t.Assembly.FullName.Contains("-Editor.dll"))
            {
                return true;
            }

            // Check if assembly is referencing the UnityEditor dll, and is not a Unity-generated assembly itself
            if (!t.Assembly.FullName.Contains("Assembly-") &&
                t.Assembly.GetReferencedAssemblies().Any(p => p.Name == "UnityEditor"))
            {
                return true;
            }

            return false;
        }
    }
}
