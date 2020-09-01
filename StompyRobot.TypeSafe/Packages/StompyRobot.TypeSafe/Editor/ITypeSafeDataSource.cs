// ReSharper disable once CheckNamespace

namespace TypeSafe.Editor
{
    /// <summary>
    /// Interface for a class that can provide a <c>TypeSafeDataUnit</c> to TypeSafe. Any non-abstract classes
    /// inheriting from this interface will be automatically instantiated by TypeSafe.
    /// </summary>
    public interface ITypeSafeDataSource
    {
        /// <summary>
        /// Called by TypeSafe during the scan process. Return a <c>TypeSafeDataUnit</c> object describing your custom data.
        /// </summary>
        /// <returns>A <c>TypeSafeDataUnit</c> object describing your custom data</returns>
        TypeSafeDataUnit GetTypeSafeDataUnit();
    }
}
