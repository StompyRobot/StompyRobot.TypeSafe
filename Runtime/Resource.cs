using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TypeSafe
{
    /// <summary>
    /// Represents a generic Unity Resource
    /// </summary>
    public interface IResource
    {
        /// <summary>
        /// Type of the resource
        /// </summary>
        Type Type { get; }

        /// <summary>
        /// Name of the resource (ie when calling unityObject.name)
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Path string that is passed to Resources.Load(...) to load this resource object.
        /// </summary>
        string Path { get; }

        /// <summary>
        /// Get the object for this resource. If the resource has not been loaded before, this will block until it is loaded.
        /// Subsequent calls to this method will return the cached reference to the resource.
        /// </summary>
        /// <returns>The resource object.</returns>
        Object Load();

        /// <summary>
        /// Unload the resource from memory and clear the cached reference.
        /// </summary>
        void Unload();
    }

    /// <summary>
    /// A type-safe reference to a unity object in the Resources folder.
    /// </summary>
    /// <typeparam name="TResource">Unity Object type of this resource</typeparam>
    public class Resource<TResource> : IResource where TResource : Object
    {
        /// <summary>
        /// Type of resource pointed to.
        /// </summary>
        public Type Type
        {
            get { return typeof (TResource); }
        }

        /// <summary>
        /// Name of the resource (ie when calling unityObject.name)
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Path string that is passed to Resources.Load(...) to load this resource object.
        /// </summary>
        public string Path { get; private set; }

        /// <summary>
        /// True if the resource has been loaded.
        /// </summary>
        public bool IsLoaded
        {
            get
            {
                BlockOnAsync();

                if (Reference != null)
                {
                    return true;
                }

                return false;
            }
        }

        private TResource Reference
        {
            get
            {
                if (_internalReference != null && _internalReference.IsAlive && ((Object)_internalReference.Target) != null)
                {
                    return (TResource) _internalReference.Target;
                }
                return null;
            }
        }

        private WeakReference _internalReference;
        private ResourceRequest _asyncRequest;

        /// <summary>
        /// Constructs a Resource pointer object with the given path
        /// </summary>
        /// <param name="name">Name of the resource (from Object.name)</param>
        /// <param name="path">Path to pass to Unity Resources.Load that will load this resource.</param>
        public Resource(string name, string path)
        {
            Name = name;
            Path = path;
        }

        /// <summary>
        /// Get the object for this resource. If the resource has not been loaded before, this will block until it is loaded.
        /// Subsequent calls to this method will return the cached reference to the resource.
        /// </summary>
        /// <seealso cref="LoadAsync"/>
        /// <returns>Resource of type <typeparamref name="TResource"/></returns>
        public TResource Load()
        {
            // Check for async requests for this resource. Calling .asset on the async
            // request will block if the loading is not complete, so we don't need to check isDone.
            //BlockOnAsync();

            // Populate cache if it is empty
            if (Reference == null)
            {
                var resource = Resources.Load<TResource>(Path);

                if (resource == null)
                {
                    Debug.LogWarning(
                        string.Format(
                            "[TypeSafe] Resource at path [{0}] is not found. Does TypeSafe need to be refreshed?", Path));
                    return null;
                }

                _internalReference = new WeakReference(resource);
            }

            return Reference;
        }

        /// <summary>
        /// Unload the resource from memory and clear the cached reference.
        /// </summary>
        public void Unload()
        {
            if (!IsLoaded)
            {
                return;
            }

            BlockOnAsync();

            if (Reference != null && !(Reference is GameObject))
            {
                Resources.UnloadAsset(Reference);
            }

            _internalReference = null;
            _asyncRequest = null;
        }

        Object IResource.Load()
        {
            return Load();
        }

        /// <summary>
        /// Returns a <see cref="UnityEngine.ResourceRequest"/> object that can be used in a yield operation. 
        /// The resource will be loaded in a background thread. Calling Load() or IsLoaded will block until async load
        /// is completed.
        /// </summary>
        /// <example>
        /// IEnumerator LoadCoroutine() {
        ///     yield return SRResources.SomeResource.LoadAsync();
        ///     Debug.Log("Loaded SomeResource");
        /// }
        /// </example>
        /// <seealso cref="Load"/>
        /// <returns></returns>
        public ResourceRequest LoadAsync()
        {
            _asyncRequest = Resources.LoadAsync<TResource>(Path);

            if (_asyncRequest.isDone && _asyncRequest.asset == null)
            {
                Debug.LogWarning(
                    string.Format(
                        "[TypeSafe] Resource at path [{0}] is not found. Does TypeSafe need to be refreshed?", Path));
            }

            return _asyncRequest;
        }

        /// <summary>
        /// Implicitly convert the resource to the Unity type, loading it if necessary.
        /// </summary>
        public static implicit operator TResource(Resource<TResource> resource)
        {
            return resource.Load();
        }

        /// <summary>
        /// Return a formatted string describing this resource pointer.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("(Resource: {0} \"{1}\")", Type.Name, Path);
        }

        /// <summary>
        /// If there is an async request in progress, block and assign it the reference fields
        /// </summary>
        private void BlockOnAsync()
        {
            if (_asyncRequest == null)
            {
                return;
            }

            _internalReference = new WeakReference(_asyncRequest.asset);
            _asyncRequest = null;
        }
    }
}
