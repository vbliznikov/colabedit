using System;
using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;

namespace CollabEdit.VersionControl.Operations.Tests
{
    [TestFixture(typeof(int), typeof(int))]
    public class TestDictionaryMergeHandler<TKey, TValue>
    {
        [TestCaseSource(typeof(Dictionary_MergeCases))]
        public void Test_Merge(IDictionary<TKey, TValue> origin, IDictionary<TKey, TValue> left, IDictionary<TKey, TValue> right,
            IDictionary<TKey, TValue> expected, ConflictResolutionOptions options, Type expectedException = null)
        {
            var mergeHandler = new DictionaryMergeHandler<TKey, TValue>();
            if (expectedException == null)
            {
                var result = mergeHandler.Merge(origin, left, right, options);

                Assert.That(result.Keys, Is.EquivalentTo(expected.Keys), "Resulting Keys should match expectation");
                Assert.That(result.Keys.All(key => result[key].Equals(expected[key])), "Resulting Values at the given Keys should be Equal");
            }
            else
                Assert.Throws(expectedException, () => mergeHandler.Merge(origin, left, right, options));
        }
    }
}