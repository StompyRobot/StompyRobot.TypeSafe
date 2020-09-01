using System.Collections.Generic;
using UnityEngine;

namespace TypeSafe
{
    /// <summary>
    /// Util methods used by TypeSafe at runtime.
    /// </summary>
    public static class TypeSafeUtil
    {
        /// <summary>
        /// Return a iterator of all resources that are assignable from <typeparamref name="TResource"/> in <paramref name="resources"/>.
        /// </summary>
        /// <typeparam name="TResource">Type of resource to return.</typeparam>
        /// <param name="resources">List of resources to check.</param>
        /// <returns>New list instance containing matching resources.</returns>
        public static IEnumerable<Resource<TResource>> GetResourcesOfType<TResource>(IEnumerable<IResource> resources)
            where TResource : Object
        {
            foreach (var r in resources)
            {
                if (r is Resource<TResource>)
                {
                    yield return (Resource<TResource>)r;
                }
            }
        }

        /// <summary>
        /// Call Unload() on all resources in list
        /// </summary>
        public static void UnloadAll(IEnumerable<IResource> resources)
        {
            foreach (IResource r in resources)
            {
                r.Unload();
            }
        }
    }
}
