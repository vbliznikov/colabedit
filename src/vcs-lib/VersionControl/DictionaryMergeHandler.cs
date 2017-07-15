using System;
using System.Linq;
using System.Collections.Generic;

using CollabEdit.VersionControl.Operations;

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

            var lEditScript = KeysEditScript<TKey>.From(origin.Keys, left.Keys);
            var rEditScript = KeysEditScript<TKey>.From(origin.Keys, right.Keys);
            var resultScript = lEditScript.Merge(rEditScript);

            foreach (TKey key in resultScript.CommonKeys)
            {
                var originValue = origin[key];
                var leftValue = left[key];
                var rightValue = right[key];

                var mergedValue = MergeUtils.Merge<TValue>(originValue, leftValue, rightValue);
                mergedDictionary.Add(key, mergedValue);
            }

            foreach (TKey key in resultScript.InsertedKeys)
            {
                var leftValue = left[key];
                var rightValue = right[key];

                var mergedValue = MergeUtils.Merge<TValue>(leftValue, rightValue);
                mergedDictionary.Add(key, mergedValue);
            }

            return mergedDictionary;
        }
    }
}