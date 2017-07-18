using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;

using CollabEdit.VersionControl.Operations;

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
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (metadata == null) throw new ArgumentException(nameof(metadata));

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
            if (sourceBranch == null) throw new ArgumentNullException(nameof(sourceBranch));
            if (sourceBranch.Head == null) throw new ArgumentException("Source branch is empty");
            if (Head == null) throw new InvalidOperationException("Current branch is empty");

            if (Head.Equals(sourceBranch.Head)) return Head;
            lock (syncRoot)
            {
                var commonAncestor = FindCommonAcestor(Head, sourceBranch.Head);

                if (commonAncestor == null) throw new MergeOperationException("Current and source branch have no common acenstor.");

                // Current branch is ahead of source branch
                if (commonAncestor.Equals(sourceBranch.Head)) return Head;

                // Simple Forward merge
                if (commonAncestor.Equals(Head))
                {
                    Head = sourceBranch.Head;
                    return Head;
                }
                // Do merge otherwise;
                TValue mergedValue = Repository.MergeHandler.Merge(commonAncestor.Value, Head.Value, sourceBranch.Head.Value,
                    ConflictResolutionOptions.RaiseException);
                // TODO Find a more suitable solution for TMeta
                TMeta meta = GreateMergeMetadata(sourceBranch, commonAncestor);
                var mergeCommit = new Commit<TValue, TMeta>(mergedValue, meta, Head, sourceBranch.Head);
                Head = mergeCommit;
            }
            return Head;
        }

        protected virtual TMeta GreateMergeMetadata(RepositoryBranch<TValue, TMeta> branch, Commit<TValue, TMeta> commonAncestor)
        {
            return default(TMeta);
        }

        protected Commit<TValue, TMeta> FindCommonAcestor(Commit<TValue, TMeta> left, Commit<TValue, TMeta> right)
        {
            var commonNodeTrace = new HashSet<Commit<TValue, TMeta>>();
            commonNodeTrace.Add(left);
            commonNodeTrace.Add(right);

            var leftBranchTrace = new HashSet<Commit<TValue, TMeta>>();
            var rightBranchTrace = new HashSet<Commit<TValue, TMeta>>();

            Commit<TValue, TMeta> commonAncestor = null;
            var leftParents = new Queue<Commit<TValue, TMeta>>();
            var rightParents = new Queue<Commit<TValue, TMeta>>();

            foreach (var parent in left.Parents)
                leftParents.Enqueue(parent);

            foreach (var parent in right.Parents)
                rightParents.Enqueue(parent);

            while (commonAncestor == null)
            {
                if (leftParents.Count > 0)
                {
                    commonAncestor = MoveUp(commonNodeTrace, leftBranchTrace, leftParents);
                    if (commonAncestor != null) break;
                }

                if (rightParents.Count > 0)
                {
                    commonAncestor = MoveUp(commonNodeTrace, rightBranchTrace, rightParents);
                    if (commonAncestor != null) break;
                }

                if (leftParents.Count == 0 && rightParents.Count == 0)
                    break; // we reached the root from both sides
            }

            return commonAncestor;
        }

        protected Commit<TValue, TMeta> MoveUp(ISet<Commit<TValue, TMeta>> commonTrace, ISet<Commit<TValue, TMeta>> branchTrace, Queue<Commit<TValue, TMeta>> parentNodes)
        {
            var next = parentNodes.Dequeue();
            if (commonTrace.Contains(next)) return next;

            commonTrace.Add(next);
            foreach (var parent in next.Parents)
            {
                // Do not traverse the same parent twise from merge nodes
                if (branchTrace.Contains(parent)) continue;
                parentNodes.Enqueue(parent);
                branchTrace.Add(parent);
            }

            return null;
        }

        public IEnumerable<Commit<TValue, TMeta>> GetHistory()
        {
            if (Head == null) yield break;

            var next = Head;
            while (next != null)
            {
                yield return next;
                next = next.Previous;
            }
        }
    }
}
