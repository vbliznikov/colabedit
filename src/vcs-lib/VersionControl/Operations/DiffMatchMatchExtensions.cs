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

        public static string ToResultText(this List<Diff> source)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            var parts = source.Where(diff => diff.operation != Operation.DELETE)
                foreach (var part in parts)
                sb.Append(part.text);

            return sb.ToString();
        }

        public static void Align(this List<Diff> source, List<Diff> other)
        {
            for (int index = 0; index < Math.Max(source.Count, other.Count); index++)
            {
                if (index > source.Count - 1) // insertion in the end of source
                {
                    var diff = other[index];
                    source.Insert(index, new Diff(diff.operation, diff.text));
                }
                if (index > other.Count - 1) // insertion in the end of other
                {
                    var diff = source[index];
                    other.Insert(index, new Diff(diff.operation, diff.text));
                }

                var left = source[index];
                var right = other[index];

                if (left.operation != right.operation && left.operation == Operation.INSERT)
                {
                    other.Insert(index, new Diff(left.operation, left.text));
                    continue;
                }
                if (left.operation != right.operation && right.operation == Operation.INSERT)
                {
                    source.Insert(index, new Diff(right.operation, right.text));
                    continue;
                }
                var leftLength = left.text.Length;
                var rightLength = right.text.Length;
                if (leftLength < rightLength)
                {
                    var newSegment = new Diff(right.operation, right.text.Substring(leftLength));
                    right.text = right.text.Substring(0, leftLength);
                    other.Insert(index + 1, newSegment);
                }
                if (rightLength < leftLength)
                {
                    var newSegment = new Diff(left.operation, left.text.Substring(rightLength));
                    left.text = left.text.Substring(0, rightLength);
                    source.Insert(index + 1, newSegment);
                }
            }
        }
    }
}