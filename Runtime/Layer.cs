namespace TypeSafe
{
    /// <summary>
    /// Wrapper class for a Unity layer name/index combo.
    /// </summary>
    public class Layer
    {
        /// <summary>
        /// Name of the sorting layer, as might be passed to <c>LayerMask.NameToLayer</c>
        /// </summary>
        public string name { get; private set; }

        /// <summary>
        /// Layer number, as assigned to <c>gameObject.layer</c>
        /// </summary>
        public int index { get; private set; }

        /// <summary>
        /// Layer mask, as might be passed to <c>Physics.Raycast</c>
        /// </summary>
        public int mask { get; private set; }

        /// <summary>
        /// Implicitly convert the <c>Layer</c> to an int that can be assigned to <c>gameObject.layer</c>;
        /// </summary>
        public static implicit operator int(Layer layer)
        {
            return layer.index;
        }

        /// <summary>
        /// Create a new <c>Layer</c> object.
        /// </summary>
        public Layer(string name, int index)
        {
            this.name = name;
            this.index = index;
            this.mask = 1 << index;
        }
    }
}
