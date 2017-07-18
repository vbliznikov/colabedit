using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;

namespace CollabEdit.VersionControl.Operations.Tests
{
    internal class Dictionary_MergeCases : IEnumerable
    {
        public IEnumerator GetEnumerator()
        {
            yield return AllEmpty();
            yield return InsertionLeft();
            yield return InsertionRight();
            yield return InsertTheSameKeyAndValueLeftAndRight();
            yield return InsertTheSameKeyAndDifferentValueLeftAndRight();
            yield return InsertDifferentKeysLeftAndRight();
            yield return ChangeValueLeft();
            yield return ChangeValueRight();
            yield return TheSameKey_ChangeValueLeftAndRightToSameValue();
            yield return TheSameKey_ChangeValueLeftAndRightToDifferentValue();
            yield return DeleteKeyLeftAndNoChangeRight();
            yield return DeleteKeyLeftAndChangeRight();
            yield return DeleteKeyRightAndNoChangeLeft();
            yield return DeleteKeyRightAndChangeLeft();
            yield return DeleteTheSameKeyLeftAndRight();
            yield return DeleteDifferentKeysLeftAndRight();
        }

        private TestCaseData AllEmpty()
        {
            var args = new object[6];
            for (int i = 0; i < 4; i++)
            {
                args[i] = new Dictionary<int, int>();
            }
            args[4] = ConflictResolutionOptions.RaiseException;
            args[5] = null;

            return new TestCaseData(args).SetName("All Empty");
        }

        private TestCaseData InsertionLeft()
        {
            var args = new object[6];
            args[0] = new Dictionary<int, int>(); // origin
            args[1] = new Dictionary<int, int>() { { 1, 0 } }; //left
            args[2] = new Dictionary<int, int>(); //right
            args[3] = new Dictionary<int, int>() { { 1, 0 } }; //result

            args[4] = ConflictResolutionOptions.RaiseException;
            args[5] = null;

            return new TestCaseData(args).SetName("Insert One Value Left");
        }

        private TestCaseData InsertionRight()
        {
            var args = new object[6];
            args[0] = new Dictionary<int, int>(); // origin
            args[1] = new Dictionary<int, int>(); //left
            args[2] = new Dictionary<int, int>() { { 1, 0 } }; //right
            args[3] = new Dictionary<int, int>() { { 1, 0 } }; //result

            args[4] = ConflictResolutionOptions.RaiseException;
            args[5] = null;

            return new TestCaseData(args).SetName("Insert one value Right");
        }

        private TestCaseData InsertTheSameKeyAndValueLeftAndRight()
        {
            var args = new object[6];
            args[0] = new Dictionary<int, int>(); // origin
            args[1] = new Dictionary<int, int>() { { 1, 0 } }; //left
            args[2] = new Dictionary<int, int>() { { 1, 0 } }; //right
            args[3] = new Dictionary<int, int>() { { 1, 0 } }; //result

            args[4] = ConflictResolutionOptions.RaiseException;
            args[5] = null;

            return new TestCaseData(args).SetName("Insert the same Key and Value Left and Right");
        }

        private TestCaseData InsertTheSameKeyAndDifferentValueLeftAndRight()
        {
            var args = new object[6];
            args[0] = new Dictionary<int, int>(); // origin
            args[1] = new Dictionary<int, int>() { { 1, 0 } }; //left
            args[2] = new Dictionary<int, int>() { { 1, 1 } }; //right
            args[3] = new Dictionary<int, int>() { }; //result

            args[4] = ConflictResolutionOptions.RaiseException;
            args[5] = typeof(MergeOperationException);

            return new TestCaseData(args).SetName("Insert the same Key but different Values Left and Right");
        }

        private TestCaseData InsertDifferentKeysLeftAndRight()
        {
            var args = new object[6];
            args[0] = new Dictionary<int, int>(); // origin
            args[1] = new Dictionary<int, int>() { { 1, 0 } };  //left
            args[2] = new Dictionary<int, int>() { { 2, 1 } }; //right
            args[3] = new Dictionary<int, int>() { { 1, 0 }, { 2, 1 } }; //result

            args[4] = ConflictResolutionOptions.RaiseException;
            args[5] = null;

            return new TestCaseData(args).SetName("Insert different Keys Left and Right");
        }

        private TestCaseData ChangeValueLeft()
        {
            var args = new object[6];
            args[0] = new Dictionary<int, int>() { { 1, 0 } }; // origin
            args[1] = new Dictionary<int, int>() { { 1, 1 } }; // left
            args[2] = new Dictionary<int, int>() { { 1, 0 } }; // right
            args[3] = new Dictionary<int, int>() { { 1, 1 } }; // result

            args[4] = ConflictResolutionOptions.RaiseException;
            args[5] = null;

            return new TestCaseData(args).SetName("Change single Value Left");
        }

        private TestCaseData ChangeValueRight()
        {
            var args = new object[6];
            args[0] = new Dictionary<int, int>() { { 1, 0 } }; // origin
            args[1] = new Dictionary<int, int>() { { 1, 0 } }; // left
            args[2] = new Dictionary<int, int>() { { 1, 1 } }; // right
            args[3] = new Dictionary<int, int>() { { 1, 1 } }; // result

            args[4] = ConflictResolutionOptions.RaiseException;
            args[5] = null;

            return new TestCaseData(args).SetName("Change single Value Right");
        }

        private TestCaseData TheSameKey_ChangeValueLeftAndRightToSameValue()
        {
            var args = new object[6];
            args[0] = new Dictionary<int, int>() { { 1, 0 } }; // origin
            args[1] = new Dictionary<int, int>() { { 1, 1 } }; // left
            args[2] = new Dictionary<int, int>() { { 1, 1 } }; // right
            args[3] = new Dictionary<int, int>() { { 1, 1 } }; // result

            args[4] = ConflictResolutionOptions.RaiseException;
            args[5] = null;

            return new TestCaseData(args).SetName("The Same Key Change Value Left & Right to the same value");
        }

        private TestCaseData TheSameKey_ChangeValueLeftAndRightToDifferentValue()
        {
            var args = new object[6];
            args[0] = new Dictionary<int, int>() { { 1, 0 } }; // origin
            args[1] = new Dictionary<int, int>() { { 1, 1 } }; // left
            args[2] = new Dictionary<int, int>() { { 1, 2 } }; // right
            args[3] = new Dictionary<int, int>() { }; // result

            args[4] = ConflictResolutionOptions.RaiseException;
            args[5] = typeof(MergeOperationException);

            return new TestCaseData(args).SetName("The Same Key Change Value Left & Right to the different values");
        }

        private TestCaseData DeleteKeyLeftAndNoChangeRight()
        {
            var args = new object[6];
            args[0] = new Dictionary<int, int>() { { 1, 0 } }; // origin
            args[1] = new Dictionary<int, int>() { }; // left
            args[2] = new Dictionary<int, int>() { { 1, 0 } }; // right
            args[3] = new Dictionary<int, int>() { }; // result

            args[4] = ConflictResolutionOptions.RaiseException;
            args[5] = null;

            return new TestCaseData(args).SetName("Delete key left, no change right");
        }

        private TestCaseData DeleteKeyLeftAndChangeRight()
        {
            var args = new object[6];
            args[0] = new Dictionary<int, int>() { { 1, 0 } }; // origin
            args[1] = new Dictionary<int, int>() { }; // left
            args[2] = new Dictionary<int, int>() { { 1, 1 } }; // right
            args[3] = new Dictionary<int, int>() { }; // result

            args[4] = ConflictResolutionOptions.RaiseException;
            args[5] = typeof(MergeOperationException);

            return new TestCaseData(args).SetName("Delete key left & change right");
        }

        private TestCaseData DeleteKeyRightAndNoChangeLeft()
        {
            var args = new object[6];
            args[0] = new Dictionary<int, int>() { { 1, 0 } }; // origin
            args[1] = new Dictionary<int, int>() { { 1, 0 } }; // left
            args[2] = new Dictionary<int, int>() { }; // right
            args[3] = new Dictionary<int, int>() { }; // result

            args[4] = ConflictResolutionOptions.RaiseException;
            args[5] = null;

            return new TestCaseData(args).SetName("Delete key right, no change left");
        }

        private TestCaseData DeleteKeyRightAndChangeLeft()
        {
            var args = new object[6];
            args[0] = new Dictionary<int, int>() { { 1, 0 } }; // origin
            args[1] = new Dictionary<int, int>() { { 1, 1 } }; // left
            args[2] = new Dictionary<int, int>() { }; // right
            args[3] = new Dictionary<int, int>() { }; // result

            args[4] = ConflictResolutionOptions.RaiseException;
            args[5] = typeof(MergeOperationException);

            return new TestCaseData(args).SetName("Delete key right & change left");
        }

        private TestCaseData DeleteTheSameKeyLeftAndRight()
        {
            var args = new object[6];
            args[0] = new Dictionary<int, int>() { { 1, 0 } }; // origin
            args[1] = new Dictionary<int, int>() { }; // left
            args[2] = new Dictionary<int, int>() { }; // right
            args[3] = new Dictionary<int, int>() { }; // result

            args[4] = ConflictResolutionOptions.RaiseException;
            args[5] = null;

            return new TestCaseData(args).SetName("Delete key left & right");
        }

        private TestCaseData DeleteDifferentKeysLeftAndRight()
        {
            var args = new object[6];
            args[0] = new Dictionary<int, int>() { { 1, 0 }, { 2, 0 }, { 3, 1 } }; // origin
            args[1] = new Dictionary<int, int>() { { 2, 0 }, { 3, 1 } }; // left
            args[2] = new Dictionary<int, int>() { { 1, 0 }, { 3, 1 } }; // right
            args[3] = new Dictionary<int, int>() { { 3, 1 } }; // result

            args[4] = ConflictResolutionOptions.RaiseException;
            args[5] = null;

            return new TestCaseData(args).SetName("Delete different keys left & right");
        }
    }
}