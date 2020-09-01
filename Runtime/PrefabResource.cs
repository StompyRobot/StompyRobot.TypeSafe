using UnityEngine;

namespace TypeSafe
{
    /// <summary>
    /// Specialized Resource for prefabs.
    /// </summary>
    public class PrefabResource : Resource<GameObject>
    {
        /// <summary>
        /// Create a PrefabResource instance.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="path"></param>
        public PrefabResource(string name, string path) : base(name, path) {}

        /// <summary>
        /// Instantiate a new instance of this prefab and return the result.
        /// </summary>
        /// <returns>Newly instantiated object.</returns>
        public GameObject Instantiate()
        {
            return Object.Instantiate(Load());
        }

        /// <summary>
        /// Instantiate a new instance of this prefab with <paramref name="parent"/>.
        /// </summary>
        /// <param name="parent">The transform the object will be parented to.</param>
        /// <returns>Newly instantiated object.</returns>
        public GameObject Instantiate(Transform parent)
        {
            return (GameObject) Object.Instantiate(Load(), parent);
        }

        /// <summary>
        /// Instantiate a new instance of this prefab with <paramref name="parent"/>.
        /// </summary>
        /// <param name="parent">The transform the object will be parented to.</param>
        /// <param name="worldPositionStays">If when assigning the parent the original world position should be maintained.</param>
        /// <returns>Newly instantiated object.</returns>
        public GameObject Instantiate(Transform parent, bool worldPositionStays)
        {
            return (GameObject) Object.Instantiate(Load(), parent, worldPositionStays);
        }

        /// <summary>
        /// Instantiate a new instance of this prefab at position and rotation and return the result.
        /// </summary>
        /// <param name="position">Position for the new object.</param>
        /// <param name="rotation">Orientation of the new object.</param>
        /// <returns>Newly instantiated object.</returns>
        public GameObject Instantiate(Vector3 position, Quaternion rotation)
        {
            return (GameObject) Object.Instantiate(Load(), position, rotation);
        }

        /// <summary>
        /// Instantiate a new instance of this prefab at <paramref name="position"/> and return the result.
        /// </summary>
        /// <param name="position">Position for the new object.</param>
        /// <returns>Newly instantiated object.</returns>
        public GameObject Instantiate(Vector3 position)
        {
            return (GameObject) Object.Instantiate(Load(), position, Quaternion.identity);
        }

        /// <summary>
        /// Instantiate a new instance of this prefab at <paramref name="position"/>, with <paramref name="rotation"/> and <paramref name="parent"/>.
        /// </summary>
        /// <param name="position">Position for the new object.</param>
        /// <param name="rotation">Orientation of the new object.</param>
        /// <param name="parent">The transform the object will be parented to.</param>
        /// <returns>Newly instantiated object.</returns>
        public GameObject Instantiate(Vector3 position, Quaternion rotation, Transform parent)
        {
            return (GameObject) Object.Instantiate(Load(), position, rotation, parent);
        }
    }
}
