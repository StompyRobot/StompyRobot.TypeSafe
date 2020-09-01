namespace TypeSafe
{
    /// <summary>
    /// Wrapper class for a Unity sorting layer name/id combo.
    /// </summary>
    public class SortingLayer
    {
        /// <summary>
        /// Name of the sorting layer, as assigned to <c>SpriteRenderer.sortingLayerName</c>
        /// </summary>
        public string name { get; private set; }

        /// <summary>
        /// Unique ID, as assigned to <c>SpriteRenderer.sortingLayerID</c>
        /// </summary>
        public int id { get; private set; }

        /// <summary>
        /// Implicitly convert the <c>SortingLayer</c> to an int that can be assigned to <c>SpriteRenderer.sortingLayerId</c>;
        /// </summary>
        public static implicit operator int(SortingLayer layer)
        {
            return layer.id;
        }

        /// <summary>
        /// Implicitly convert the <c>SortingLayer</c> object to a string that can be assigned to <c>SpriteRenderer.sortingLayerName</c>;
        /// </summary>
        public static implicit operator string(SortingLayer layer)
        {
            return layer.name;
        }

        /// <summary>
        /// Create a new <c>SortingLayer</c> object.
        /// </summary>
        public SortingLayer(string name, int id)
        {
            this.name = name;
            this.id = id;
        }
    }
}
