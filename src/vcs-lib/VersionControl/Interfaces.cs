using System;
using System.Collections.Generic;

namespace CollabEdit.VersionControl
{
    /// <summary>
    /// Represents the Repository of TValue values stored with TMeta metadata
    /// </summary>
    public interface IRepository<TValue, TMeta>
    {
        /// <summary>
        /// Collection of Branches the Repository keeps track of.
        /// </summary>
        /// <returns>Readonly collection of <see creaf="IBranch<TValue, TMeta>">Branches</see></returns>
        ICollection<IBranch<TValue, TMeta>> Branches { get; }

        ///<summary>
        /// Get the Branch by name
        /// <param name="name">Name of the Branch to get</param>
        /// <returns>Instance of IBranch interface</returns>
        ///</summary>
        IBranch<TValue, TMeta> this[string name] { get; }

        /// <summary>
        /// Serves as a trunk the new Branches start from.
        /// </summary>
        /// <returns>Branch set as Current.</returns>
        IBranch<TValue, TMeta> MasterBranch { get; }

        /// <summary>
        /// Creates new Branch started from MasterBranch Head
        /// </summary>
        /// <param name="name">Name os the new <see cref="IBranch<TValue, TMeta>">Branch</see></param>
        /// <returns>Created branch</returns>
        IBranch<TValue, TMeta> CreateBranch(string name);

        /// <summary>
        /// Deletes existing Branch.
        /// </summary>
        /// <param name="name">Name os the Branch to delete.</param>
        void DeleteBranch(string name);
    }

    /// <summary>
    /// Represent the named version sequence (Branch) in Repository
    /// </summary>
    public interface IBranch<TValue, TMeta>
    {
        /// <summary>
        /// The Repository this Branch belongs to
        /// </summary>
        /// <returns>Reference to Respository</returns>
        IRepository<TValue, TMeta> Repository { get; }

        /// <summary>
        /// Name of the Branch
        /// </summary>
        /// <returns></returns>
        string Name { get; }

        /// <summary>
        /// The most recent Commit to this Branch
        /// </summary>
        /// <returns></returns>
        Commit<TValue, TMeta> Head { get; }

        /// <summary>
        /// Store the Value with Metadata
        /// </summary>
        /// <param name="value">Value to store</param>
        /// <param name="meta">Metada to store along with Value</param>
        /// <returns></returns>
        Commit<TValue, TMeta> Commit(TValue value, TMeta meta);

        /// <summary>
        /// Merge the state of the source Branch into current Branch
        /// </summary>
        /// <param name="branch"></param>
        /// <returns>Commit with Merged value</returns>
        Commit<TValue, TMeta> MergeWith(IBranch<TValue, TMeta> sourceBranch);
    }
}