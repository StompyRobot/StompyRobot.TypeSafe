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
        /// Return a list of all resources that are assignable from <typeparamref name="TResource"/> in <paramref name="resources"/>.
        /// </summary>
        /// <typeparam name="TResource">Type of resource to return.</typeparam>
        /// <param name="resources">List of resources to check.</param>
        /// <returns>New list instance containing matching resources.</returns>
        public static List<Resource<TResource>> GetResourcesOfType<TResource>(IList<IResource> resources)
            where TResource : Object
        {
            var t = typeof (TResource);
            var lst = new List<Resource<TResource>>();

            for (var i = 0; i < resources.Count; i++)
            {
                var r = resources[i];

                if (t == r.Type)
                {
                    lst.Add((Resource<TResource>) r);
                }
            }

            return lst;
        }

        /// <summary>
        /// Call Unload() on all resources in list
        /// </summary>
        public static void UnloadAll(IList<IResource> resources)
        {
            for (var i = 0; i < resources.Count; i++)
            {
                resources[i].Unload();
            }
        }
    }
}
