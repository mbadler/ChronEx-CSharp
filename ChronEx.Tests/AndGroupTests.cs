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
    public class AndGroupTests
    {

        [TestMethod]
        public void AndGroup_OneLevelAST_Created()
        {

            var script =
@"(
a
b
)";
            var g = new ChronExParser();
            var tree = g.ParsePattern(script);
            AssetABAndGroup((AndGroupElement)tree.Statements[0]);
            var h = tree;

        }
        [TestMethod]
        public void AndGroup_AST_SingleGroupInMiddleOfOtherElements()
        {

            var script =
@"g
(
a
b
)
h";
            var g = new ChronExParser();
            var tree = g.ParsePattern(script);
            Assert.AreEqual(3, tree.Statements.Count);
            var st1 = (SpecifiedEventNameSelector)tree.Statements[0];
            Assert.AreEqual("g", st1.EventName);
            AssetABAndGroup((AndGroupElement)tree.Statements[1]);
            var st2 = (SpecifiedEventNameSelector)tree.Statements[2];
            Assert.AreEqual("h", st2.EventName);


        }
        [TestMethod]
        public void AndGroup_AST_MultiLevelEmbededGroups()
        {

            var script =
@"g
(
    (
        a
        b
    )
    f
    (
        a
        b
    )
)
h";
            var g = new ChronExParser();
            var tree = g.ParsePattern(script);
            Assert.AreEqual(3, tree.Statements.Count);
            var st1 = (SpecifiedEventNameSelector)tree.Statements[0];
            Assert.AreEqual("g", st1.EventName);
            var Rootandgroup = (AndGroupElement)tree.Statements[1];
            Assert.AreEqual(3, Rootandgroup.Statements.Count);
            AssetABAndGroup((AndGroupElement)(Rootandgroup.Statements[0]));
            var subst1 = (SpecifiedEventNameSelector)Rootandgroup.Statements[1];
            Assert.AreEqual("f", subst1.EventName);
            AssetABAndGroup((AndGroupElement)(Rootandgroup.Statements[2]));
            var st2 = (SpecifiedEventNameSelector)tree.Statements[2];
            Assert.AreEqual("h", st2.EventName);


        }

        


        [TestMethod]
        public void AndGroup_AST_GroupWithStar()
        {

            var script =
@"(
a
b
)*"
;
            var g = new ChronExParser();
            var tree = g.ParsePattern(script);
            Assert.AreEqual(1, tree.Statements.Count);
            var st1 = (SymbolQuantifierSyntax)tree.Statements[0];
            Assert.AreEqual('*',st1.QuantifierSymbol);

         

            AssetABAndGroup((AndGroupElement)st1.ContainedElement);
            //var subst1 = (SpecifiedEventNameSelector)Rootandgroup.Statements[1];
            //Assert.AreEqual("f", subst1.EventName);
            //AssetABAndGroup((AndGroupElement)(Rootandgroup.Statements[2]));
            //var st2 = (SpecifiedEventNameSelector)tree.Statements[2];
            //Assert.AreEqual("h", st2.EventName);


        }
        [TestMethod]
        public void AndGroup_AST_SubEmbededGroupWithNumericQuan()
        {

            var script =
@"(
    a
    (
        a
        b
    ){1,2}
    b
)*"
;
            var g = new ChronExParser();
            var tree = g.ParsePattern(script);
            Assert.AreEqual(1, tree.Statements.Count);
            var st1 = (SymbolQuantifierSyntax)tree.Statements[0];
            Assert.AreEqual('*', st1.QuantifierSymbol);

            var subg = ((NumericQuantifierSyntax)((AndGroupElement)st1.ContainedElement).Statements[1]);
            Assert.AreEqual(2, subg.MaxOccours);

             


        }

        [TestMethod]
        public void AndGroup_SimpleGroupExactMatch()
        {
            var script =
@"(
a
b
)";
            var events = TestUtils.ChronListFromString("a b");
            var matches = ChronEx.Matches(script, events);
            matches.AssertMatchesAreEqual(
@"a,b");
}


        [TestMethod]
        public void AndGroup_SingleThenAndGroup()
        {
            var script =
@"a
(
b
c
)";
            var events = TestUtils.ChronListFromString("a b c");
            var matches = ChronEx.Matches(script, events);
            matches.AssertMatchesAreEqual(
@"a,b,c");
        }

        [TestMethod]
        public void AndGroup_SingleGroupSingle()
        {
            var script =
@"a
(
b
c
)
d";
            var events = TestUtils.ChronListFromString("a b c d");
            var matches = ChronEx.Matches(script, events);
            matches.AssertMatchesAreEqual(
@"a,b,c,d");

        }

        [TestMethod]
        public void AndGroup_SingleGroupSingle_multipleTimes()
        {
            var script =
@"a
(
b
c
)
d";
            var events = TestUtils.ChronListFromString("a b c d a b c d a b c d");
            var matches = ChronEx.Matches(script, events);
            matches.AssertMatchesAreEqual(
@"a,b,c,d
a,b,c,d
a,b,c,d");

        }

        [TestMethod]
        public void AndGroup_MultiNestedAndLevels()
        {
            var script =
@"a
(
    (
        b
        c
        (
            d
            (
                e
                f
            )
            g
            (
                h
                i
            )
        )
        j
    )
    k
    l
)
m";
            var events = TestUtils.ChronListFromString("a b c d e f g h i j k l m");
            var matches = ChronEx.Matches(script, events);
            matches.AssertMatchesAreEqual(
@"a,b,c,d,e,f,g,h,i,j,k,l,m");
 

        }

        [TestMethod]
        public void AndGroup_MultipleGroupSingle_DoesNotMatchEntirely()
        {
            var script =
@"a
(
b
c
e
)";
            var events = TestUtils.ChronListFromString("a b c d a b c e a b c d");
            var matches = ChronEx.Matches(script, events);
            matches.AssertMatchesAreEqual(
@"a,b,c,e");

        }

        [TestMethod]
        public void AndGroup_MNumericQuantifier()
        {
            var script =
@"(
a
b
c
){5}";
            var events = TestUtils.ChronListFromString("a b c a b c a b c a b c a b c a b c");
            var matches = ChronEx.Matches(script, events);
            matches.AssertMatchesAreEqual(
@"a,b,c,a,b,c,a,b,c,a,b,c,a,b,c");

        }

        [TestMethod]
        public void AndGroup_StarQuantifier()
        {
            var script =
@"(
a
b
c
)*";
            var events = TestUtils.ChronListFromString("a b c a b c a b c a b c a b c a b c");
            var matches = ChronEx.Matches(script, events);
            matches.AssertMatchesAreEqual(
@"a,b,c,a,b,c,a,b,c,a,b,c,a,b,c,a,b,c");

        }

        [TestMethod]
        public void AndGroup_StarQuantifierEmbededInANumericMinimum()
        {
            var script =
@"(
    a*
    b
){4,}";
            var events = TestUtils.ChronListFromString("a a a a b a b a a b a a b q r e ");
            var matches = ChronEx.Matches(script, events);
            matches.AssertMatchesAreEqual(
@"a,a,a,a,b,a,b,a,a,b,a,a,b");

        }

        [TestMethod]
        public void AndGroup_TrailingSelectorAfterNumericGroup()
        {
            var script =
@"(
    a*
    b
){,4}
.
q";
            var events = TestUtils.ChronListFromString("a a a a b a b a a b a a b r q");
            var matches = ChronEx.Matches(script, events);
            matches.AssertMatchesAreEqual(
@"a,a,a,a,b,a,b,a,a,b,a,a,b,r,q");

        }

        [TestMethod]
        public void AndGroup_AST_NegatedAndGroupNotAllowed()
        {
            var script =
@"a
!(
    b
    c
)";
            var g = new ChronExParser();
            try
            {
                g.ParsePattern(script);
            }
            catch(ParserException ex)
            {
                if(ex.Message == "Negated Groups are not allowed")
                {
                    return;
                }
               
            }
            Assert.Fail("Expected a 'Negated Groups are not allowed' exception");
            

        }


        [TestMethod]
        public void AndGroup_MultipleConsecutiveAndGroups()
        {
            var script =
@"(
    a
    b
    c
)
(
    a
    b
    c
)
(
    a
    b
    c
)";
            var events = TestUtils.ChronListFromString("y a b c a b c a b c a b c a b c a b c");
            var matches = ChronEx.Matches(script, events);
            matches.AssertMatchesAreEqual(
@"a,b,c,a,b,c,a,b,c
a,b,c,a,b,c,a,b,c");

        }
        private static void AssetABAndGroup(AndGroupElement tree)
        {
            
            Assert.AreEqual(2, tree.Statements.Count);
            var it1 = (SpecifiedEventNameSelector)tree.Statements[0];
            var it2 = (SpecifiedEventNameSelector)tree.Statements[1];
            Assert.AreEqual("a", it1.EventName);
            Assert.AreEqual("b", it2.EventName);
        }
    }
}