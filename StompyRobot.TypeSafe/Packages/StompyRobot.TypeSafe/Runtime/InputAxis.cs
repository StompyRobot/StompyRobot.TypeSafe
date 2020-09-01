using UnityEngine;

namespace TypeSafe
{
    /// <summary>
    /// Wrapper for a Unity input axis
    /// </summary>
    public sealed class InputAxis
    {
        /// <summary>
        /// Name of the axis that can be passed to <c>Input.GetAxis(name)</c>.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// The current value for this input axis.
        /// Equivalent to calling <c>Input.GetAxis</c>
        /// </summary>
        public float Value
        {
            get { return Input.GetAxis(Name); }
        }

        /// <summary>
        /// The current raw value for this input axis.
        /// Equivalent to calling <c>Input.GetAxisRaw</c>
        /// </summary>
        public float RawValue
        {
            get { return Input.GetAxisRaw(Name); }
        }

        /// <summary>
        /// The current pressed state for this input axis.
        /// Equivalent to calling <c>Input.GetButton</c>.
        /// </summary>
        public bool IsPressed
        {
            get { return Input.GetButton(Name); }
        }

        /// <summary>
        /// Was this button pressed down this frame.
        /// Equivalent to calling <c>Input.GetButtonDown</c>.
        /// </summary>
        public bool Down
        {
            get { return Input.GetButtonDown(Name); }
        }

        /// <summary>
        /// Was this button released this frame.
        /// Equivalent to calling <c>Input.GetButtonUp</c>.
        /// </summary>
        public bool Up
        {
            get { return Input.GetButtonUp(Name); }
        }

        /// <summary>
        /// Create a new <c>InputAxis</c> object
        /// </summary>
        public InputAxis(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Implicitly convert the <c>InputAxis</c> to a string that can be used in <c>Input.GetAxisX()</c> methods.
        /// </summary>
        public static implicit operator string(InputAxis axis)
        {
            return axis.Name;
        }
    }
}
