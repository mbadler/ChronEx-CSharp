using ChronEx.Models;
using ChronEx.Processor;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChronEx.Tests
{
    [TestClass]
    public class EventStreamTests
    {

        [TestMethod]
        public void EventStream_WhileLoop()
        {
            var n = new EventStream(TestUtils.ChronListFromString("a b c d e f"));
            var res = new List<IChronologicalEvent>();
            while (n.MoveNext())
            {
                res.Add(n.Current);
            }
            var expected = TestUtils.ChronListFromString("a b c d e f");
            AssertEventListsAreSame(res, expected);

        }

        





        [TestMethod]
        public void EventStream_SpeculatorContinues()
        {
            var n = new EventStream(TestUtils.ChronListFromString("a b c d e f"));
            var res = new List<IChronologicalEvent>();
            n.MoveNext();
            res.Add(n.Current);
            var speculator = n.CreateSpeculator();

            while (speculator.MoveNext())
            {
                res.Add(speculator.Current);
            }
            var expected = TestUtils.ChronListFromString("a b c d e f");
            
            AssertEventListsAreSame(res, expected);

        }

        [TestMethod]
        public void EventStream_SubSpeculatorContinues()
        {
            var n = new EventStream(TestUtils.ChronListFromString("a b c d e f"));
            var res = new List<IChronologicalEvent>();
            n.MoveNext();
            res.Add(n.Current);
            var speculator = n.CreateSpeculator();
            speculator.MoveNext();
            res.Add(speculator.Current);
            var subspec = speculator.CreateSpeculator();
            while (subspec.MoveNext())
            {
                res.Add(subspec.Current);
            }
            var expected = TestUtils.ChronListFromString("a b c d e f");

            AssertEventListsAreSame(res, expected);

        }

        [TestMethod]
        public void EventStream_SpeculatorSolidifes()
        {
            var n = new EventStream(TestUtils.ChronListFromString("a b c d e f"));
            var res = new List<IChronologicalEvent>();
            n.MoveNext();
            res.Add(n.Current);
            var speculator = n.CreateSpeculator();
            for (int i = 0; i < 3; i++)
            {
                speculator.MoveNext();
                res.Add(speculator.Current);
            }
            speculator.EndApplyAll();
            while (n.MoveNext())
            {
                res.Add(n.Current);
            }
            var expected = TestUtils.ChronListFromString("a b c d e f");

            AssertEventListsAreSame(res, expected);

        }

        [TestMethod]
        public void EventStream_SubSpeculatorSolidifes()
        {
            var n = new EventStream(TestUtils.ChronListFromString("a b c d e f"));
            var res = new List<IChronologicalEvent>();
            n.MoveNext();
            res.Add(n.Current);
            var speculator = n.CreateSpeculator();
            speculator.MoveNext();
            res.Add(speculator.Current);
            var subspec = speculator.CreateSpeculator();
            subspec.MoveNext();
            res.Add(subspec.Current);
            subspec.EndApplyAll();
            speculator.MoveNext();
            res.Add(speculator.Current);
            speculator.EndApplyAll();
             
            while (n.MoveNext())
            {
                res.Add(n.Current);
            }
            var expected = TestUtils.ChronListFromString("a b c d e f");

            AssertEventListsAreSame(res, expected);

        }

        [TestMethod]
        public void EventStream_SpeculatorAfterOtherSpeculator()
        {
            var n = new EventStream(TestUtils.ChronListFromString("a b c d e f"));
            var res = new List<IChronologicalEvent>();
            n.MoveNext();
            res.Add(n.Current);
            var speculator = n.CreateSpeculator();
            
                speculator.MoveNext();
                res.Add(speculator.Current);

            speculator.EndApplyAll();
            n.MoveNext();

                res.Add(n.Current);
            speculator = n.CreateSpeculator();

            speculator.MoveNext();
            res.Add(speculator.Current);
            speculator.EndApplyAll();
            n.MoveNext();

            res.Add(n.Current);
            n.MoveNext();

            res.Add(n.Current);
            var expected = TestUtils.ChronListFromString("a b c d e f");

            AssertEventListsAreSame(res, expected);

        }

        [TestMethod]
        public void EventStream_SpeculatorRejectsAll()
        {
            var n = new EventStream(TestUtils.ChronListFromString("a b c d e f"));
            var res = new List<IChronologicalEvent>();

            n.MoveNext();
            res.Add(n.Current);
            var speculator = n.CreateSpeculator();
            while (speculator.MoveNext())
            {
                res.Add(speculator.Current);
            }

            speculator.EndDiscardAll();
            while (n.MoveNext())
            {
                res.Add(n.Current);
            }
            var expected = TestUtils.ChronListFromString("a b c d e f").ToList();
            expected.AddRange(expected.Skip(1).ToList());

            AssertEventListsAreSame(res, expected);
        }

        [TestMethod]
        public void EventStream_SpeculatorRejectsAllMidway()
        {
            var n = new EventStream(TestUtils.ChronListFromString("a b c d e f"));
            var res = new List<IChronologicalEvent>();

            n.MoveNext();
            res.Add(n.Current);
            var speculator = n.CreateSpeculator();
            speculator.MoveNext();
            
                res.Add(speculator.Current);
            

            speculator.EndDiscardAll();
            while (n.MoveNext())
            {
                res.Add(n.Current);
            }
            var expected = TestUtils.ChronListFromString("a b c d e f").ToList();
            expected.Insert(2, expected[1]);

            AssertEventListsAreSame(res, expected);
        }
        [TestMethod]
        public void EventStream_SpeculatorAfterPreviousSpeculatorReject()
        {
            var n = new EventStream(TestUtils.ChronListFromString("a b c d e f"));
            var res = new List<IChronologicalEvent>();
            var speculator = n.CreateSpeculator();
            while (speculator.MoveNext())
            {
                res.Add(speculator.Current);
            }
            speculator.EndDiscardAll();
            speculator = n.CreateSpeculator();
            while (speculator.MoveNext())
            {
                res.Add(speculator.Current);
            }
            speculator.EndDiscardAll();
            
            
            var expected = TestUtils.ChronListFromString("a b c d e f").ToList();
            expected.AddRange(expected);

            AssertEventListsAreSame(res, expected);
        }

        private static void AssertEventListsAreSame(List<IChronologicalEvent> res, IEnumerable<ChronologicalEvent> expected)
        {
            var expectedList = expected.ToList();
            CollectionAssert.AreEqual(expectedList, res, Comparer<IChronologicalEvent>.Create((x, y) =>
            {
                return x.Describe().CompareTo(y.Describe());
            }));
        }

    }
}



