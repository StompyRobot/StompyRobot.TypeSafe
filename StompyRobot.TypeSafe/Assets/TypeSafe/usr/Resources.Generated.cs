// ------------------------------------------------------------------------------
//  _______   _____ ___ ___   _   ___ ___ 
// |_   _\ \ / / _ \ __/ __| /_\ | __| __|
//   | |  \ V /|  _/ _|\__ \/ _ \| _|| _| 
//   |_|   |_| |_| |___|___/_/ \_\_| |___|
// 
// This file has been generated automatically by TypeSafe.
// Any changes to this file may be lost when it is regenerated.
// https://www.stompyrobot.uk/tools/typesafe
// 
// TypeSafe Version: 1.5.0
// 
// ------------------------------------------------------------------------------



public sealed class SRResources {
    
    private SRResources() {
    }
    
    private const string _tsInternal = "1.5.0";
    
    private static global::System.Collections.Generic.IReadOnlyList<global::TypeSafe.IResource> @__ts_internal_resources = new global::TypeSafe.IResource[0];
    
    public sealed class SubFolder2 {
        
        private SubFolder2() {
        }
        
        public static global::TypeSafe.Resource<global::UnityEngine.Material> TestMaterial1 {
            get {
                return ((global::TypeSafe.Resource<global::UnityEngine.Material>)(@__ts_internal_resources[0]));
            }
        }
        
        private static global::System.Collections.Generic.IReadOnlyList<global::TypeSafe.IResource> @__ts_internal_resources = new global::TypeSafe.IResource[] {
                new global::TypeSafe.Resource<global::UnityEngine.Material>("TestMaterial1", "SubFolder2/TestMaterial1")};
        
        /// <summary>
        /// Return a read-only list of all resources in this folder.
        /// This method has a very low performance cost, no need to cache the result.
        /// </summary>
        /// <returns>A list of resource objects in this folder.</returns>
        public static global::System.Collections.Generic.IReadOnlyList<global::TypeSafe.IResource> GetContents() {
            return @__ts_internal_resources;
        }
        
        private static global::System.Collections.Generic.IReadOnlyList<global::TypeSafe.IResource> @__ts_internal_recursiveLookupCache;
        
        /// <summary>
        /// Return a list of all resources in this folder and all sub-folders.
        /// The result of this method is cached, so subsequent calls will have very low performance cost.
        /// </summary>
        /// <returns>A list of resource objects in this folder and sub-folders.</returns>
        public static global::System.Collections.Generic.IReadOnlyList<global::TypeSafe.IResource> GetContentsRecursive() {
            if ((@__ts_internal_recursiveLookupCache != null)) {
                return @__ts_internal_recursiveLookupCache;
            }
            global::System.Collections.Generic.List<global::TypeSafe.IResource> tmp = new global::System.Collections.Generic.List<global::TypeSafe.IResource>();
            tmp.AddRange(GetContents());
            @__ts_internal_recursiveLookupCache = tmp;
            return @__ts_internal_recursiveLookupCache;
        }
        
        /// <summary>
        /// Return an iterator of all resources in this folder of type <typeparamref name="TResource"> (does not include sub-folders)
        /// This method does not cache the result, so you should cache the result yourself if you will use it often. Convert to a list first if it will be iterated over multiple time.
        /// </summary>
        /// <returns>A list of <typeparamref>TResource</typeparamref> objects in this folder.</returns>
        public static global::System.Collections.Generic.IEnumerable<global::TypeSafe.Resource<TResource>> GetContents<TResource>()
            where TResource : global::UnityEngine.Object {
            return global::TypeSafe.TypeSafeUtil.GetResourcesOfType<TResource>(GetContents());
        }
        
        /// <summary>
        /// Return a iterator of all resources in this folder of type <typeparamref name="TResource">, including sub-folders.
        /// This method does not cache the result, so you should cache the result yourself if you will use it often. Convert to a list first if it will be iterated over multiple time.
        /// </summary>
        /// <returns>A list of <typeparamref>TResource</typeparamref> objects in this folder and sub-folders.</returns>
        public static global::System.Collections.Generic.IEnumerable<global::TypeSafe.Resource<TResource>> GetContentsRecursive<TResource>()
            where TResource : global::UnityEngine.Object {
            return global::TypeSafe.TypeSafeUtil.GetResourcesOfType<TResource>(GetContentsRecursive());
        }
        
        /// <summary>
        /// Call Unload() on every loaded resource in this folder.
        /// </summary>
        public static void UnloadAll() {
            global::TypeSafe.TypeSafeUtil.UnloadAll(GetContents());
        }
        
        /// <summary>
        /// Call Unload() on every loaded resource in this folder and subfolders.
        /// </summary>
        private void UnloadAllRecursive() {
            global::TypeSafe.TypeSafeUtil.UnloadAll(GetContentsRecursive());
        }
        
        /// <summary>
        /// Clears any internal lists of assets that were cached by <see cref="GetContentsRecursive"/>.
        /// </summary>
        /// <returns>A list of resource objects in this folder.</returns>
        internal static void ClearCache() {
            @__ts_internal_recursiveLookupCache = null;
        }
    }
    
    /// <summary>
    /// Return a read-only list of all resources in this folder.
    /// This method has a very low performance cost, no need to cache the result.
    /// </summary>
    /// <returns>A list of resource objects in this folder.</returns>
    public static global::System.Collections.Generic.IReadOnlyList<global::TypeSafe.IResource> GetContents() {
        return @__ts_internal_resources;
    }
    
    private static global::System.Collections.Generic.IReadOnlyList<global::TypeSafe.IResource> @__ts_internal_recursiveLookupCache;
    
    /// <summary>
    /// Return a list of all resources in this folder and all sub-folders.
    /// The result of this method is cached, so subsequent calls will have very low performance cost.
    /// </summary>
    /// <returns>A list of resource objects in this folder and sub-folders.</returns>
    public static global::System.Collections.Generic.IReadOnlyList<global::TypeSafe.IResource> GetContentsRecursive() {
        if ((@__ts_internal_recursiveLookupCache != null)) {
            return @__ts_internal_recursiveLookupCache;
        }
        global::System.Collections.Generic.List<global::TypeSafe.IResource> tmp = new global::System.Collections.Generic.List<global::TypeSafe.IResource>();
        tmp.AddRange(GetContents());
        tmp.AddRange(SubFolder2.GetContentsRecursive());
        @__ts_internal_recursiveLookupCache = tmp;
        return @__ts_internal_recursiveLookupCache;
    }
    
    /// <summary>
    /// Return an iterator of all resources in this folder of type <typeparamref name="TResource"> (does not include sub-folders)
    /// This method does not cache the result, so you should cache the result yourself if you will use it often. Convert to a list first if it will be iterated over multiple time.
    /// </summary>
    /// <returns>A list of <typeparamref>TResource</typeparamref> objects in this folder.</returns>
    public static global::System.Collections.Generic.IEnumerable<global::TypeSafe.Resource<TResource>> GetContents<TResource>()
        where TResource : global::UnityEngine.Object {
        return global::TypeSafe.TypeSafeUtil.GetResourcesOfType<TResource>(GetContents());
    }
    
    /// <summary>
    /// Return a iterator of all resources in this folder of type <typeparamref name="TResource">, including sub-folders.
    /// This method does not cache the result, so you should cache the result yourself if you will use it often. Convert to a list first if it will be iterated over multiple time.
    /// </summary>
    /// <returns>A list of <typeparamref>TResource</typeparamref> objects in this folder and sub-folders.</returns>
    public static global::System.Collections.Generic.IEnumerable<global::TypeSafe.Resource<TResource>> GetContentsRecursive<TResource>()
        where TResource : global::UnityEngine.Object {
        return global::TypeSafe.TypeSafeUtil.GetResourcesOfType<TResource>(GetContentsRecursive());
    }
    
    /// <summary>
    /// Call Unload() on every loaded resource in this folder.
    /// </summary>
    public static void UnloadAll() {
        global::TypeSafe.TypeSafeUtil.UnloadAll(GetContents());
    }
    
    /// <summary>
    /// Call Unload() on every loaded resource in this folder and subfolders.
    /// </summary>
    private void UnloadAllRecursive() {
        global::TypeSafe.TypeSafeUtil.UnloadAll(GetContentsRecursive());
    }
    
    /// <summary>
    /// Clears any internal lists of assets that were cached by <see cref="GetContentsRecursive"/>.
    /// </summary>
    /// <returns>A list of resource objects in this folder.</returns>
    public static void ClearCache() {
        @__ts_internal_recursiveLookupCache = null;
        SubFolder2.ClearCache();
    }
}
