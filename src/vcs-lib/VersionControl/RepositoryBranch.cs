using System;
using System.Collections.Generic;
using System.Threading;

namespace CollabEdit.VersionControl
{

    public class RepositoryBranch<TValue, TMeta>
    {
        private object syncRoot = new Object();
        public RepositoryBranch() { }

        public RepositoryBranch(string name, Repository<TValue, TMeta> repository, Commit<TValue, TMeta> head)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Name may not be null or empty string.");
            if (repository == null) throw new ArgumentNullException(nameof(repository));

            Name = name;
            Repository = repository;
            Head = head;
        }

        public Repository<TValue, TMeta> Repository { get; }

        public string Name { get; }

        public Commit<TValue, TMeta> Head { get; protected set; }

        public Commit<TValue, TMeta> Commit(TValue value, TMeta metadata)
        {
            if (Head != null && Head.Value.Equals(value))
                return Head; // Value was not changed, so no need to create a new version.

            lock (syncRoot)
            {
                var commit = new Commit<TValue, TMeta>(value, metadata, Head);
                Head = commit;
                return commit;
            }
        }

        public virtual Commit<TValue, TMeta> MergeWith(RepositoryBranch<TValue, TMeta> sourceBranch)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Commit<TValue, TMeta>> GetHistory()
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
