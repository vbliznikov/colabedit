using System.Collections.Generic;

namespace VersionControl
{

    public class Repository<T>
    {
        public Commit<T> Head { get; protected set; }

        public Commit<T> Commit(T value, string comment)
        {
            var commit = new Commit<T>(value, new CommitMetadata(comment), Head);
            Head = commit;
            return commit;
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
