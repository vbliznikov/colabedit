using System;
using System.Collections.Generic;

namespace VersionControl
{

    public class RepositoryBranch<T>
    {
        public RepositoryBranch() { }

        public RepositoryBranch(string name, Commit<T> head)
        {
            Name = name;
            Head = head;
        }

        public string Name { get; }

        public Commit<T> Head { get; protected set; }

        public Commit<T> Commit(T value, string comment)
        {
            if (Head != null && Head.Value.Equals(value))
                return Head; // Value was not changed, so no need to create a new version.

            var commit = new Commit<T>(value, new CommitMetadata(comment), Head);
            Head = commit;
            return commit;
        }

        public RepositoryBranch<T> BranchFrom(string name, Commit<T> commit)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Name may not be null or empty string.");
            if (commit == null) throw new ArgumentNullException(nameof(commit));

            return new RepositoryBranch<T>(name, commit);
        }

        public IEnumerable<Commit<T>> GetHistory()
        {
            if (Head == null) yield break;

            var next = Head;
            while (next != null)
            {
                yield return next;
                next = next.Parent;
            }
        }
    }
}
