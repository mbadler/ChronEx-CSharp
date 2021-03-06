﻿using ChronEx.Models;
using ChronEx.Models.AST;
using ChronEx.Parser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChronEx.Tests
{
    [TestClass]
    public class SymbolQuantifierElementTests
    {
        [TestMethod]
        public void Quant_Symbol_CorrectASTCreatedWithNegate()
        {
            var n = new ChronExParser().ParsePattern("!abc+").Statements.ToList();

            Assert.IsTrue(n[0] is SymbolQuantifierSyntax);
            var b = (SymbolQuantifierSyntax)n[0];
            Assert.IsTrue(b.ContainedElement is NegatedSyntax);
            var neg = ((NegatedSyntax)b.ContainedElement);
            Assert.IsTrue(neg.ContainedElement is SpecifiedEventNameSelector);

        }

        [TestMethod]
        public void Quant_Symbol_CorrectSymbolAssigned()
        {
            var n = new ChronExParser().ParsePattern("abc+").Statements.ToList();

            Assert.IsTrue(n[0] is SymbolQuantifierSyntax);
            var b = (SymbolQuantifierSyntax)n[0];
            Assert.AreEqual('+', b.QuantifierSymbol);

        }



        [TestMethod]
        public void Quant_Symbol_PlusExists()
        {
            var script =
@"a
b+
c";
            var events = TestUtils.ChronListFromString(@"a b b b b c a b b c a c a b c");
            var matches = ChronEx.Matches(script, events);
            matches.AssertMatchesAreEqual(
@"a,b,b,b,b,c
a,b,b,c
a,b,c");

            
        }

        [TestMethod]
        public void Quant_Symbol_PlusWithRegexOr()
        {
            var script =
@"a
/b|c/+
d";
            var events = GetQuanTestSet();
            var matches = ChronEx.Matches(script, events);
            matches[0].CapturedEvents.AssertMatchesAreEqual("a,b,b,b,b,c,c,c,c,d");
            matches[1].CapturedEvents.AssertMatchesAreEqual("a,b,b,c,c,d");
            matches[2].CapturedEvents.AssertMatchesAreEqual("a,b,c,d");
            Assert.AreEqual(3, matches.Count);
        }

        [TestMethod]
        public void Quant_Symbol_Star_AllOneEvents()
        {
            var events = TestUtils.ChronListFromString("a a a a a a");
            var script = "a*";
            var matches = ChronEx.Matches(script, events);
            matches.AssertMatchesAreEqual("a,a,a,a,a,a");

        }

        [TestMethod]
        public void Quant_Symbol_Star()
        {
            var script =
@"b
a*
d";
            var events = TestUtils.ChronListFromString("b a d b d b a a a d b g d");
            var matches = ChronEx.Matches(script, events);
            matches[0].CapturedEvents.AssertMatchesAreEqual("b,a,d");
            matches[1].CapturedEvents.AssertMatchesAreEqual("b,d");
            matches[2].CapturedEvents.AssertMatchesAreEqual("b,a,a,a,d");
            Assert.AreEqual(3, matches.Count);
        }

        [TestMethod]
        public void Quant_Symbol_Star_simple()
        {
            var script =
@"b
a*
d";
            var events = TestUtils.ChronListFromString("b a d");
            var matches = ChronEx.Matches(script, events);
            matches[0].CapturedEvents.AssertMatchesAreEqual("b,a,d");
            
            Assert.AreEqual(1, matches.Count);
        }



        [TestMethod]
        public void Quant_Symbol_QuestionMark()
        {
            var script =
@"b
a?
d";
            var events = GetQuanTestSet();
            var matches = ChronEx.Matches(script, events);
            matches.AssertMatchesAreEqual(
@"b,a,d
b,d");
           
        }

        [TestMethod]
        public void Quant_Symbol_QuestionMark_Chained()
        {
            var script =
@"b
a?
b?
c?
d?
e";
            var events = TestUtils.ChronListFromString("b b d e");
            var matches = ChronEx.Matches(script, events);
            matches.AssertMatchesAreEqual("b,b,d,e");

          
        }


        [TestMethod]
        public void Quant_Symbol_Star_Chained_otherEvents()
        {
            var script =
@"b*
a*
b*
c*
d*
e*
f*";
            var events = TestUtils.ChronListFromString("b a a b b c c d d e f b a b b c d e");
            var matches = ChronEx.Matches(script, events);
            matches.AssertMatchesAreEqual(
@"b,a,a,b,b,c,c,d,d,e,f
b,a,b,b,c,d,e");


        }

        [TestMethod]
        public void Quant_Symbol_Star_InterleavedWithOther()
        {
            var script =
@"b*
a?
a
b*
c+";
            var events = TestUtils.ChronListFromString("b a a b b c c d d e f b a a b b c d e");
            var matches = ChronEx.Matches(script, events);
            matches.AssertMatchesAreEqual(
@"b,a,a,b,b,c,c
b,a,a,b,b,c");


        }

        public void Quant_Symbol_Star_Simple()
        {
            var script =
@"b*";
            var events = TestUtils.ChronListFromString("b b b a a a b b b");
            var matches = ChronEx.Matches(script, events);
            matches.AssertMatchesAreEqual(
@"b,a,a,b,b,c,c
b,a,a,b,b,c");


        }

        [TestMethod]
        public void Quant_Symbol_QuestionMark_Chained_otherEvents()
        {
            var script =
@"b
a+
b?
b
c+
d+
e
f?";
            var events = TestUtils.ChronListFromString("b a a b b c c d d e f b a b b c d e");
            var matches = ChronEx.Matches(script, events);
            matches.AssertMatchesAreEqual(
@"b,a,a,b,b,c,c,d,d,e,f
b,a,b,b,c,d,e");


        }

        [TestMethod]
        public void Quant_Symbol_QuestionMark_simple()
        {
            var script =
@"b
a?
d";
            var events = TestUtils.ChronListFromString("b a d");
            var matches = ChronEx.Matches(script, events);
            matches[0].CapturedEvents.AssertMatchesAreEqual("b,a,d");

            Assert.AreEqual(1, matches.Count);
        }
        [TestMethod]
        public void Quant_Symbol_WithDashNoCapture()
        {
            var events = TestUtils.ChronListFromString("a a b a a a b a b b a c");
            
            var script =
            @"a
-!b+
b";
            var matches = ChronEx.Matches(script, events);

            matches.AssertMatchesAreEqual(
@"a,b
a,b");
        }

        [TestMethod]
        public void Quant_Symbol_WithDashNoCapture_Simple()
        {
            var events = TestUtils.ChronListFromString("a b b");

            var script =
            @"a
-!b+
b";
            var matches = ChronEx.Matches(script, events);

            matches.AssertNoMatches();
 
        }
        [TestMethod]
        public void Quant_Symbol_Plus_WithNeage_NoFullMatch()
        {
            var events = TestUtils.ChronListFromString("a c");

            var script =
            @"a
!b+
b";
            var matches = ChronEx.Matches(script, events);

            matches.AssertNoMatches();

        }

        [TestMethod]
        public void Quant_Symbol_Star_MultipleNegations()
        {
            var events = TestUtils.ChronListFromString("O F O F O A B C D F O A B O");

            var script =
            @"O
!F*
F";
            var matches = ChronEx.Matches(script, events);

            matches.AssertMatchesAreEqual(
@"O,F
O,F
O,A,B,C,D,F");

        }

        [TestMethod]
        public void Quant_Symbol_Star_MultipleNegations_FindOutliers()
        {
            var events = TestUtils.ChronListFromString("O F O F O A B C D F O A B O");

            var script =
            @"O
!/F|O/*
O";
            var matches = ChronEx.Matches(script, events);

            matches.AssertMatchesAreEqual(
@"O,A,B,O");

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