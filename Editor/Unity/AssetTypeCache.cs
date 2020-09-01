using System;
using System.Collections.Generic;

namespace TypeSafe.Editor.Unity
{
    static class AssetTypeCache
    {
        private static Dictionary<string, Type> _typeCache;

        public static bool GetAssetType(string path, out Type t)
        {
            LoadCache();
            return _typeCache.TryGetValue(path, out t);
        }

        public static void PostAssetType(string path, Type t)
        {
            LoadCache();
            _typeCache[path] = t;
        }

        public static bool ClearAssetType(string path)
        {
            LoadCache();
            return _typeCache.Remove(path);
        }

        public static void ClearCache()
        {
            TSLog.Log(LogCategory.Trace, "[AssetTypeCache] Clearing Asset Type Cache");
			if (_typeCache != null)
			{
				_typeCache.Clear();
            }

            if (TypeCache.Instance != null)
            {
                TypeCache.Instance.Contents = new TypeCacheEntry[0];
                TypeCache.Instance.Save();
            }
        }

        public static void SaveCache()
        {
            if (_typeCache == null)
                return;

            TSLog.Log(LogCategory.Trace, "[AssetTypeCache] Saving Asset Type Cache");

            var l = new List<TypeCacheEntry>(_typeCache.Count);

            foreach (var kv in _typeCache)
            {
                l.Add(new TypeCacheEntry()
                {
                    Path = kv.Key,
                    Type = kv.Value.AssemblyQualifiedName
                });
            }

            TypeCache.Instance.Contents = l.ToArray();
            TypeCache.Instance.Save();
        }

        private static void LoadCache()
        {
            if (_typeCache != null)
                return;

            TSLog.Log(LogCategory.Trace, "[AssetTypeCache] Loading Asset Type Cache");

            _typeCache = new Dictionary<string, Type>();

            var cache = TypeCache.Instance.Contents;

            if (cache == null)
            {
                return;
            }

            for (var i = 0; i < cache.Length; i++)
            {
                var type = Type.GetType(cache[i].Type, false);

                if (type == null)
                {
                    TSLog.LogWarning(LogCategory.Trace,
	                    string.Format("[AssetTypeCache] Type from cache was not found (path: {0}, type: {1})", cache[i].Path, cache[i].Type));
                    continue;
                }

                if (_typeCache.ContainsKey(cache[i].Path))
                {
                    TSLog.LogError(LogCategory.Trace, string.Format("[AssetTypeCache] Duplicate path in type cache ({0})", cache[i].Path));
                    continue;
                }

                _typeCache.Add(cache[i].Path, type);
            }
        }
    }
}
