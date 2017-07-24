using System;
using System.Linq;
using System.Collections.Generic;

using Google.DiffMatchPatch;

namespace CollabEdit.VersionControl.Operations
{
    public class MergeScript
    {
        public MergeScript(List<Diff> left, List<Diff> right)
        {
            Left = left; Right = right;
            Comparer = StringComparer.Ordinal;
        }
        public List<Diff> Left { get; }
        public List<Diff> Right { get; }

        public IEqualityComparer<String> Comparer { get; }

        public IEnumerable<Tuple<int, string, string>> Conflicts { get; protected set; }

        public List<Diff> Result { get; protected set; }

        protected List<Diff> ALeft { get; set; }
        protected List<Diff> ARight { get; set; }

        bool HasConflict
        {
            get
            {
                if (Conflicts == null)
                    Merge();
                return Conflicts.Any();
            }
        }
        public void Merge()
        {
            AlignScripts();
            var conflicts = new List<Tuple<int, string, string>>();
            var result = new List<Diff>(ALeft.Count);
            for (int i = 0; i < ALeft.Count; i++)
            {
                var left = ALeft[i]; var right = ARight[i];
                if (left.operation == Operation.INSERT && !Comparer.Equals(left.text, right.text))
                {
                    conflicts.Add(new Tuple<int, string, string>(i, left.text, right.text));
                    result.Add(new Diff(Operation.INSERT, new String('!', left.text.Length)));
                    continue;
                }
                if (left.operation == right.operation) // either Equlity or Deleteion
                    result.Add(left);

                if (left.operation == Operation.DELETE)
                    result.Add(left);
                if (right.operation == Operation.DELETE)
                    result.Add(right);
            }

            Conflicts = conflicts;
            Result = result;
        }

        protected void AlignScripts()
        {
            ALeft = Left.Clone();
            ARight = Right.Clone();

            for (int index = 0; index < Math.Max(ALeft.Count, ARight.Count); index++)
            {
                if (index > ALeft.Count - 1) // insertion in the end of ARight
                {
                    var diff = ARight[index];
                    ALeft.Insert(index, diff);
                }
                if (index > ARight.Count - 1) // insertion in the end of ALeft
                {
                    var diff = ALeft[index];
                    ARight.Insert(index, diff);
                }

                var left = ALeft[index];
                var right = ARight[index];

                if (left.operation == Operation.INSERT && left.operation != right.operation)
                {
                    ARight.Insert(index, new Diff(left.operation, left.text));
                    continue;
                }
                if (right.operation == Operation.INSERT && left.operation != right.operation)
                {
                    ALeft.Insert(index, new Diff(right.operation, right.text));
                    continue;
                }
                var leftLength = left.text.Length;
                var rightLength = right.text.Length;

                if (leftLength < rightLength)
                {
                    var newSegment = new Diff(right.operation, right.text.Substring(leftLength));
                    right.text = right.text.Substring(0, leftLength);
                    ARight.Insert(index + 1, newSegment);
                }
                if (rightLength < leftLength)
                {
                    var newSegment = new Diff(left.operation, left.text.Substring(rightLength));
                    left.text = left.text.Substring(0, rightLength);
                    ALeft.Insert(index + 1, newSegment);
                }
            }
        }
    }
}