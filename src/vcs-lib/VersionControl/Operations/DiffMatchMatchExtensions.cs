using System;
using System.Collections.Generic;
using System.Linq;

using Google.DiffMatchPatch;

namespace CollabEdit.VersionControl.Operations
{
    public static class DiffExtensions
    {
        public static int Cost(this List<Diff> source)
        {
            Dictionary<Operation, int> operationCost =
                new Dictionary<Operation, int> { { Operation.EQUAL, 0 }, { Operation.DELETE, 1 }, { Operation.INSERT, 1 } };
            const int charCost = 1;

            return source
                .Select(el => operationCost[el.operation] * charCost * el.text.Length)
                .Sum();
        }

        public static Diff Clone(this Diff source)
        {
            return new Diff(source.operation, source.text);
        }

        public static List<Diff> Clone(this List<Diff> source)
        {
            return source.Select(el => el.Clone()).ToList();
        }

        public static string ToText(this List<Diff> source)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            var parts = source.Where(diff => diff.operation != Operation.DELETE);
            foreach (var part in parts)
                sb.Append(part.text);

            return sb.ToString();
        }

        public static List<Diff> MergeWith(this List<Diff> source, List<Diff> other)
        {
            var comparer = StringComparer.Ordinal;
            var leftScript = source.Clone();
            var rightScript = other.Clone();
            leftScript.AlignWith(rightScript);

            var result = new List<Diff>(leftScript.Count);
            bool hasConflicts = false;

            for (int i = 0; i < leftScript.Count; i++)
            {
                var left = leftScript[i]; var right = rightScript[i];
                // After Alignment all Insertions has corresponding Pair
                if (left.operation == Operation.INSERT && !comparer.Equals(left.text, right.text))
                {
                    hasConflicts = true;
                    break;
                }
                if (left.operation == right.operation) // either Insert, Equlity or Deleteion
                    result.Add(left);

                if (left.operation == Operation.DELETE) // delete from either side results in delete
                    result.Add(left);
                if (right.operation == Operation.DELETE)
                    result.Add(right);
            }


            List<Diff> ResolveConflict(List<Diff> left, List<Diff> right)
            {
                int leftCost = left.Cost();
                int rightCost = right.Cost();

                if (leftCost > rightCost) return left;
                if (rightCost > leftCost) return right;

                var rnd = new Random().Next(2);
                if (rnd == 1)
                    return left;
                else
                    return right;
            }

            return hasConflicts ? ResolveConflict(source, other) : result;
        }

        internal static void AlignWith(this List<Diff> source, List<Diff> other)
        {
            var leftList = source;
            var rightList = other;

            for (int index = 0; index < Math.Max(leftList.Count, rightList.Count); index++)
            {
                if (index > leftList.Count - 1) // insertion in the end of rightList
                {
                    var diff = rightList[index];
                    leftList.Insert(index, diff);
                    break;
                }
                if (index > rightList.Count - 1) // insertion in the end of leftList
                {
                    var diff = leftList[index];
                    rightList.Insert(index, diff);
                    break;
                }

                var left = leftList[index];
                var right = rightList[index];

                if (left.operation != right.operation && left.operation == Operation.INSERT)
                {
                    rightList.Insert(index, left);
                    continue;
                }
                if (left.operation != right.operation && right.operation == Operation.INSERT)
                {
                    leftList.Insert(index, right);
                    continue;
                }

                var leftLength = left.text.Length;
                var rightLength = right.text.Length;

                Action<List<Diff>, int, int> splitSegment = (list, i, length) =>
                {
                    var segment = list[i];
                    var newSegment = new Diff(segment.operation, segment.text.Substring(length));
                    segment.text = segment.text.Substring(0, length);
                    list.Insert(i + 1, newSegment);
                };

                if (leftLength < rightLength)
                {
                    splitSegment(rightList, index, leftLength);
                }
                if (rightLength < leftLength)
                {
                    splitSegment(leftList, index, rightLength);
                }
            }
        }
    }
}