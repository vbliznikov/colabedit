using System;
using System.Linq;
using System.Collections.Generic;

namespace CollabEdit.VersionControl
{
    public class DictionaryMergeHandler<TKey, TValue> : IMergeHandler<IDictionary<TKey, TValue>>
    {
        public IDictionary<TKey, TValue> Merge(IDictionary<TKey, TValue> origin, IDictionary<TKey, TValue> left, IDictionary<TKey, TValue> right)
        {
            // Empty dictionaries left and right
            if (left.Count == 0 && right.Count == 0)
                return left;

            var mergedDictionary = new Dictionary<TKey, TValue>();

            var commonKeys = new HashSet<TKey>(origin.Keys);
            commonKeys.IntersectWith(left.Keys);
            commonKeys.IntersectWith(right.Keys);

            // Deal with key insertion and deletion
            var adLeft = left.Keys.Except(origin.Keys);
            var adRight = right.Keys.Except(origin.Keys);


            MergeCommonKeys(origin, left, right, mergedDictionary, commonKeys);

            if (origin.Count == commonKeys.Count && left.Count == commonKeys.Count && right.Count == commonKeys.Count)
                return mergedDictionary;

            return mergedDictionary;
        }

        private static void MergeCommonKeys(IDictionary<TKey, TValue> commonBase, IDictionary<TKey, TValue> left,
            IDictionary<TKey, TValue> right, Dictionary<TKey, TValue> mergedDictionary, HashSet<TKey> commonKeys)
        {
            foreach (var key in commonKeys)
            {
                var origin = commonBase[key];
                var leftValue = left[key];
                var rightValue = right[key];

                var mergeValue = MergeUtils.Merge(origin, leftValue, rightValue);
                mergedDictionary.Add(key, mergeValue);
            }
        }
    }
}