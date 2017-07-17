using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using CollabEdit.VersionControl.Operations;

namespace CollabEdit.VersionControl
{
    public class Repository<TValue, TMeta>
    {
        const string defaultBranchName = "master";
        private Dictionary<string, RepositoryBranch<TValue, TMeta>> _branches = new Dictionary<string, RepositoryBranch<TValue, TMeta>>();
        public Repository()
        {
            CurrentBranch = new RepositoryBranch<TValue, TMeta>(defaultBranchName, this, null);
            _branches.Add(CurrentBranch.Name, CurrentBranch);
            MergeHandler = new DumbMergehandler<TValue>();
        }
        public Repository(IMergeHandler<TValue> mergeHandler) : this()
        {
            if (mergeHandler == null) throw new ArgumentNullException(nameof(mergeHandler));
            MergeHandler = mergeHandler;
        }

        internal IMergeHandler<TValue> MergeHandler { get; }

        public RepositoryBranch<TValue, TMeta> CurrentBranch { get; protected set; }

        public ICollection<RepositoryBranch<TValue, TMeta>> Branches
        {
            get { return _branches.Values; }
        }

        public RepositoryBranch<TValue, TMeta> this[string name]
        {
            get { return _branches[name]; }
        }

        public RepositoryBranch<TValue, TMeta> CreateBranch(string name)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Branch name may not be null or empty string.");
            if (CurrentBranch.Head == null) throw new InvalidOperationException("Can't create new Branch from an empty one.");

            var branch = new RepositoryBranch<TValue, TMeta>(name, this, CurrentBranch.Head);
            _branches.Add(branch.Name, branch);

            return branch;
        }

        public void DeleteBranch(string name)
        {
            _branches.Remove(name);
        }
    }
}
