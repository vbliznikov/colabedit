using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace VersionControl
{
    public class Repository<TValue, TMeta>
    {
        const string defaultBranchName = "master";
        private Dictionary<string, RepositoryBranch<TValue, TMeta>> _branches = new Dictionary<string, RepositoryBranch<TValue, TMeta>>();
        public Repository()
        {
            CurrentBranch = new RepositoryBranch<TValue, TMeta>(defaultBranchName, this, null);
            _branches.Add(CurrentBranch.Name, CurrentBranch);
        }

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
