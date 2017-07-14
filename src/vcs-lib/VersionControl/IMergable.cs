using System;

namespace CollabEdit.VersionControl
{
    /// <summary>
    /// Inreface for objects which may suppport simple merge operation
    /// </summary>
    public interface IMergable<T>
    {
        /// <summary>
        /// Merge state of ther objects with its own state
        /// </summary>
        /// <param name="other">Other obejct to merge state with</param>
        /// <throws><see cref="MergeOperationException">MergeOperationException</see> may be thrown when merge oeration is not possible</throws>
        /// <returns>New instance wich incapsulates combined state</returns>
        T Merge(T other);
    }

    /// <summary>
    /// Interface for coplex objects to support merge scenarios through Diff against the common base state
    /// </summary>
    public interface ITwoWayMergable<T>
    {
        T Merge(T commonBase, T other);
    }

    /// <summary>
    /// Interface for external Merge Operation providers
    /// </summary>
    public interface IMergeHandler<T>
    {
        T Merge(T commonBase, T left, T right);
    }

    /// <summary>
    /// Thrown when merge operation results in conflict which can not be resolved
    /// </summary>
    public class MergeOperationException : InvalidOperationException
    {
        public MergeOperationException(string message) : base(message) { }
    }
}