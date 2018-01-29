using ChronEx.Models;
using ChronEx.Models.AST;
using ChronEx.Parser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChronEx.Tests
{
    [TestClass]
    public class NumericQuantifierElementTests
    {
        
        [TestMethod]
        public void Quant_Num_AST_FullCreated()
        {
            var events = GetQuanTestSet();
            var script = "abc{1,2}";
            var g = new ChronExParser();
            var tree = g.ParsePattern(script);
            Assert.IsInstanceOfType(tree.Statements[0],typeof( NumericQuantifierSyntax));
            var a = (NumericQuantifierSyntax)tree.Statements[0];
            Assert.AreEqual(1, a.MinOccours);
            Assert.AreEqual(2, a.MaxOccours);
        }

        [TestMethod]
        public void Quant_Num_AST_OneNumSameMaxAndMin()
        {
            var events = GetQuanTestSet();
            var script = "a{3}"; 
            var g = new ChronExParser();
            var tree = g.ParsePattern(script);
            Assert.IsInstanceOfType(tree.Statements[0], typeof(NumericQuantifierSyntax));
            var a = (NumericQuantifierSyntax)tree.Statements[0];
            Assert.AreEqual(3, a.MinOccours);
            Assert.AreEqual(3, a.MaxOccours);

        }

        [TestMethod]
        public void Quant_Num_AST_MinOnly()
        {
            var events = GetQuanTestSet();
            var script = "abc{3,}";
            var g = new ChronExParser();
            var tree = g.ParsePattern(script);
            Assert.IsInstanceOfType(tree.Statements[0], typeof(NumericQuantifierSyntax));
            var a = (NumericQuantifierSyntax)tree.Statements[0];
            Assert.AreEqual(3, a.MinOccours);
            Assert.AreEqual(int.MaxValue, a.MaxOccours);
        }

        [TestMethod]
        public void Quant_Num_AST_MaxOnly()
        {
            var events = GetQuanTestSet();
            var script = "abc{,5}";
            var g = new ChronExParser();
            var tree = g.ParsePattern(script);
            Assert.IsInstanceOfType(tree.Statements[0], typeof(NumericQuantifierSyntax));
            var a = (NumericQuantifierSyntax)tree.Statements[0];
            Assert.AreEqual(0, a.MinOccours);
            Assert.AreEqual(5, a.MaxOccours);
        }

        [TestMethod]
        public void Quant_Num_MinExact_Single()
        {
            var script =
@"b{3,}";
            var events = TestUtils.ChronListFromString("b b b");
            var matches = ChronEx.Matches(script, events);
            matches.AssertMatchesAreEqual(
@"b,b,b");

        }

        [TestMethod]
        public void Quant_Num_MinAndMax_Single()
        {
            var script =
@"b{3,4}";
            var events = TestUtils.ChronListFromString("b b b b b");
            var matches = ChronEx.Matches(script, events);
            matches.AssertMatchesAreEqual(
@"b,b,b,b");

        }

        [TestMethod]
        public void Quant_Num_Max_Single()
        {
            var script =
@"b{,4}";
            var events = TestUtils.ChronListFromString("b b b b");
            var matches = ChronEx.Matches(script, events);
            matches.AssertMatchesAreEqual(
@"b,b,b,b");

        }
        [TestMethod]
        public void Quant_Num_MinAndMax_SameLetterContinues()
        {
            var script =
@"b{2,4}";
            var events = TestUtils.ChronListFromString("b b b b b b");
            var matches = ChronEx.Matches(script, events);
            matches.AssertMatchesAreEqual(
@"b,b,b,b
b,b");

        }

        [TestMethod]
        public void Quant_Num_Minofzero_PassesOn()
        {
            var script =
@"b{0,4}
c";
            var events = TestUtils.ChronListFromString("c d");
            var matches = ChronEx.Matches(script, events);
            matches.AssertMatchesAreEqual(
@"c");

        }

        [TestMethod]
        public void Quant_Num_SpecifiedOne()
        {
            var script =
@"b{1}";
            var events = TestUtils.ChronListFromString("b b b b");
            var matches = ChronEx.Matches(script, events);
            matches.AssertMatchesAreEqual(
@"b
b
b
b");
        }
            [TestMethod]
        public void Quant_Num_OpenendedAtEndOfOther()
        {
            var script =
@"c{1,3}
b{1,}";
            var events = TestUtils.ChronListFromString("c c c b b b b");
            var matches = ChronEx.Matches(script, events);
            matches.AssertMatchesAreEqual(
@"c,c,c,b,b,b,b");

        }
        [TestMethod]
        public void Quant_Num_OpenendedRepeat()
        {
            var script =
@"c{1,3}
b{1,}";
            var events = TestUtils.ChronListFromString("c c c b b b b c c b");
            var matches = ChronEx.Matches(script, events);
            matches.AssertMatchesAreEqual(
@"c,c,c,b,b,b,b
c,c,b");

        }





        public IEnumerable<ChronologicalEvent> GetQuanTestSet()
        {
            var TestSet =
        @"a,10/17/2017 0:01
a,10/17/2017 0:02
a,10/17/2017 0:03
a,10/17/2017 0:04
b,10/17/2017 0:05
b,10/17/2017 0:06
b,10/17/2017 0:07
b,10/17/2017 0:08
c,10/17/2017 0:09
c,10/17/2017 0:10
c,10/17/2017 0:11
c,10/17/2017 0:12
d,10/17/2017 0:13
d,10/17/2017 0:14
d,10/17/2017 0:15
d,10/17/2017 0:16
a,10/17/2017 0:17
a,10/17/2017 0:18
b,10/17/2017 0:19
b,10/17/2017 0:20
c,10/17/2017 0:21
c,10/17/2017 0:22
d,10/17/2017 0:23
d,10/17/2017 0:24
a,10/17/2017 0:25
b,10/17/2017 0:26
c,10/17/2017 0:27
d,10/17/2017 0:28
c,10/17/2017 0:29
b,10/17/2017 0:30
a,10/17/2017 0:31
d,10/17/2017 0:32
c,10/17/2017 0:33
b,10/17/2007 0:34
d,10/17/2017 0:35
b,10/17/2017 0:36
b,10/17/2017 0:37
a,10/17/2017 0:38
a,10/17/2017 0:39
d,10/17/2017 0:40"
;
            return TestUtils.SplitLogsStringsIntoChronEventList(TestSet);
        }
    }
}