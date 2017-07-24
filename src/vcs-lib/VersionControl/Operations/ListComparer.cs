using System;
using System.Collections.Generic;
using System.Linq;

namespace CollabEdit.VersionControl.Operations
{
    /// <summary>
    /// Compare to ordered sequences and return the list of DiifSegments.
    /// </summary>
    /// <remarks>Unfinished.</remarks>
    public class ListComparer<T>
    {
        public ListComparer(IEqualityComparer<T> comparer)
        {
            if (comparer == null) throw new ArgumentNullException(nameof(comparer));
            Comparer = comparer;
        }
        public IEqualityComparer<T> Comparer { get; }
        public List<DiffSegnment<T>> Compare(List<T> left, List<T> right)
        {
            List<DiffSegnment<T>> result = null;

            if (left.Count == 0 && right.Count == 0)
                return new List<DiffSegnment<T>>();

            if (left.Count == 0) // insert all frorm right items
            {
                result = right.Select(item => new DiffSegnment<T>(EditOperation.Insert, item)).ToList();
                return result;
            }

            if (right.Count == 0) // delete all items from left
            {
                result = left.Select(item => new DiffSegnment<T>(EditOperation.Delete, item)).ToList();
                return result;
            }

            var leftList = left;
            var rightList = right;

            int headLength = GetCommonHeadLength(leftList, rightList);
            if (headLength > 0)
            {
                result = leftList.GetRange(0, headLength)
                    .Select(item => new DiffSegnment<T>(EditOperation.Equl, item)).ToList();

                if (leftList.Count == rightList.Count && rightList.Count == headLength) // the Sequences equals
                    return result;

                // trim start
                leftList = leftList.GetRange(headLength, leftList.Count - headLength);
                rightList = leftList.GetRange(headLength, rightList.Count - headLength);
            }

            if (leftList.Count == 0) // right sequence contains left from the start
            {
                result.AddRange
                (
                    rightList.Select(item => new DiffSegnment<T>(EditOperation.Insert, item))
                );
                return result;
            }

            if (rightList.Count == 0) // left sequence conatins right from the start
            {
                result.AddRange
                (
                   leftList.Select(item => new DiffSegnment<T>(EditOperation.Delete, item))
                );
                return result;
            }

            int tailLength = GetCommonTailLength(leftList, rightList);
            List<DiffSegnment<T>> tailResult = null;
            if (tailLength > 0)
            {
                tailResult = leftList.GetRange(leftList.Count - tailLength, tailLength)
                    .Select(item => new DiffSegnment<T>(EditOperation.Equl, item)).ToList();

                // trim common tail
                leftList = leftList.GetRange(0, leftList.Count - tailLength);
                rightList = rightList.GetRange(0, rightList.Count - tailLength);
            }

            if (leftList.Count == 0) // right sequence contains left reminder at the end
            {
                result.AddRange
                (
                    rightList.Select(item => new DiffSegnment<T>(EditOperation.Insert, item))
                        .Concat(tailResult)
                );
                return result;
            }

            if (rightList.Count == 0)
            {
                result.AddRange
                (
                    rightList.Select(item => new DiffSegnment<T>(EditOperation.Delete, item))
                        .Concat(tailResult)
                );

                return result;
            }

            return result;
        }

        protected int GetCommonHeadLength(IList<T> left, IList<T> right)
        {
            int commonLength = Math.Min(left.Count, right.Count);
            int index;
            for (index = 0; index < commonLength; index++)
            {
                if (!Comparer.Equals(left[index], right[index]))
                    break;
            }

            return index;
        }

        protected int GetCommonTailLength(IList<T> left, IList<T> right)
        {
            int commonLength = Math.Min(left.Count, right.Count);
            int index;
            for (index = commonLength - 1; index >= 0; index--)
            {
                if (!Comparer.Equals(left[index], right[index]))
                    break;
            }

            return commonLength - index;
        }
    }

    internal abstract class PartialListComparer<T>
    {
        protected PartialListComparer(IEqualityComparer<T> comparer)
        {
            if (comparer == null) throw new ArgumentNullException(nameof(comparer));
            Comparer = comparer;
        }
        public IEqualityComparer<T> Comparer { get; }
        public abstract void Compare(IList<T> left, IList<T> right, out Tuple<IList<DiffSegnment<T>>, IList<T>, IList<T>> result);
    }

    internal class ListHeadComparer<T> : PartialListComparer<T>
    {
        public ListHeadComparer(IEqualityComparer<T> comparer) : base(comparer) { }
        public override void Compare(IList<T> left, IList<T> right, out Tuple<IList<DiffSegnment<T>>, IList<T>, IList<T>> result)
        {
            if (left == null) throw new ArgumentNullException(nameof(left));
            if (right == null) throw new ArgumentNullException(nameof(left));

            System.Diagnostics.Debug.Assert(left.Count > 0, "Left sequence expected to be not empty");
            System.Diagnostics.Debug.Assert(right.Count > 0, "Right sequence expected to be not empty");

            int commonLength = Math.Min(left.Count, right.Count);
            int index;
            for (index = 0; index < commonLength; index++)
            {
                if (!Comparer.Equals(left[index], right[index]))
                    break;
            }
            IList<DiffSegnment<T>> diff;
            var leftList = left;
            var rightList = right;

            if (index == 0) // optimization
            {
                diff = new List<DiffSegnment<T>>(0);
                result = new Tuple<IList<DiffSegnment<T>>, IList<T>, IList<T>>(diff, leftList, rightList);
                return;
            }

            diff = leftList.Take(index)
                .Select(el => new DiffSegnment<T>(EditOperation.Equl, el))
                .ToList();
            leftList = left.Skip(index).ToList();
            rightList = right.Skip(index).ToList();

            if (leftList.Count == 0)
            {
                diff = diff.Concat
                (
                    rightList.Select(el => new DiffSegnment<T>(EditOperation.Insert, el))
                )
                .ToList();
            }
            if (rightList.Count == 0)
            {
                diff = diff.Concat
                (
                    leftList.Select(el => new DiffSegnment<T>(EditOperation.Delete, el))
                )
                .ToList();
            }

            result = new Tuple<IList<DiffSegnment<T>>, IList<T>, IList<T>>(diff, leftList, rightList);
        }
    }

    internal class ListTailComparer<T> : PartialListComparer<T>
    {
        public ListTailComparer(IEqualityComparer<T> comparer) : base(comparer) { }
        public override void Compare(IList<T> left, IList<T> right, out Tuple<IList<DiffSegnment<T>>, IList<T>, IList<T>> result)
        {
            if (left == null) throw new ArgumentNullException(nameof(left));
            if (right == null) throw new ArgumentNullException(nameof(left));

            System.Diagnostics.Debug.Assert(left.Count > 0, "Left sequence expected to be not empty");
            System.Diagnostics.Debug.Assert(right.Count > 0, "Right sequence expected to be not empty");

            int commonLength = Math.Min(left.Count, right.Count);
            int index;
            for (index = commonLength - 1; index >= 0; index--)
            {
                if (!Comparer.Equals(left[index], right[index]))
                    break;
            }

            var tailLength = commonLength - 1 - index;

            IList<DiffSegnment<T>> diff;
            var leftList = left;
            var rightList = right;

            if (tailLength == 0) // optimization
            {
                diff = new List<DiffSegnment<T>>(0);
                result = new Tuple<IList<DiffSegnment<T>>, IList<T>, IList<T>>(diff, leftList, rightList);
                return;
            }

            diff = leftList.Skip(leftList.Count - tailLength)
                .Select(el => new DiffSegnment<T>(EditOperation.Equl, el))
                .ToList();
            leftList = leftList.Take(leftList.Count - tailLength).ToList();
            rightList = rightList.Take(leftList.Count - tailLength).ToList();

            if (leftList.Count == 0)
            {
                diff = rightList.Select(el => new DiffSegnment<T>(EditOperation.Insert, el))
                    .Concat(diff)
                    .ToList();
            }
            if (rightList.Count == 0)
            {
                diff = leftList.Select(el => new DiffSegnment<T>(EditOperation.Delete, el))
                    .Concat(diff)
                    .ToList();
            }

            result = new Tuple<IList<DiffSegnment<T>>, IList<T>, IList<T>>(diff, leftList, rightList);
        }
    }
}