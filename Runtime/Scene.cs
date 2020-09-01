using UnityEngine;
using UnityEngine.SceneManagement;

namespace TypeSafe
{
    /// <summary>
    /// Wrapper class for a Unity Scene name/index combo.
    /// </summary>
    public sealed class Scene
    {
        /// <summary>
        /// Name of the scene, as it is passed to Application.LoadLevel(string)
        /// </summary>
        // ReSharper disable once InconsistentNaming
#pragma warning disable IDE1006 // Naming Styles
        public string name { get; private set; }

        /// <summary>
        /// Scene index, as usually passed to Application.LoadLevel(int)
        /// </summary>
        public int index { get; private set; }
#pragma warning restore IDE1006 // Naming Styles

        /// <summary>
        /// Implicitly convert the Scene to an int that can be passed to Application.LoadLevel(int)
        /// </summary>
        public static implicit operator int(Scene scene)
        {
            return scene.index;
        }

        /// <summary>
        /// Create a new Scene object.
        /// </summary>
        public Scene(string name, int index)
        {
            this.name = name;
            this.index = index;
        }

        /// <summary>
        /// Calls <c>Application.LoadLevel(...)</c>.
        /// </summary>
        public void Load()
        {
            SceneManager.LoadScene(index, LoadSceneMode.Single);
        }

        /// <summary>
        /// Calls <c>Application.LoadLevelAdditive(...)</c>.
        /// </summary>
        public void LoadAdditive()
        {
            SceneManager.LoadScene(index, LoadSceneMode.Additive);
        }

        /// <summary>
        /// Calls <c>Application.LoadLevelAdditiveAsync(...)</c>.
        /// </summary>
        public AsyncOperation LoadAdditiveAsync()
        {
            return SceneManager.LoadSceneAsync(index, LoadSceneMode.Additive);
        }

        /// <summary>
        /// Calls <c>Application.LoadLevelAsync(...)</c>.
        /// </summary>
        public AsyncOperation LoadAsync()
        {
            return SceneManager.LoadSceneAsync(index, LoadSceneMode.Single);
        }

        /// <summary>
        /// Calls <c>Application.LoadLevelAsync(...)</c>.
        /// </summary>
        public AsyncOperation UnloadAsync(UnloadSceneOptions options = UnloadSceneOptions.None)
        {
            return SceneManager.UnloadSceneAsync(index, options);
        }
    }
}
