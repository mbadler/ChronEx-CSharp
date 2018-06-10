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
    public class OrGroupTests
    {

        [TestMethod]
        public void OrGroup_OneLevelAST_Created()
        {

            var script =
@"[
a
b
]";
            var g = new ChronExParser();
            var tree = g.ParsePattern(script);
            AssetABOrGroup((OrGroupElement)tree.Statements[0]);
            var h = tree;

        }
        [TestMethod]
        public void OrGroup_AST_SingleGroupInMiddleOfOtherElements()
        {

            var script =
@"g
[
a
b
]
h";
            var g = new ChronExParser();
            var tree = g.ParsePattern(script);
            Assert.AreEqual(3, tree.Statements.Count);
            var st1 = (SpecifiedEventNameSelector)tree.Statements[0];
            Assert.AreEqual("g", st1.EventName);
            AssetABOrGroup((OrGroupElement)tree.Statements[1]);
            var st2 = (SpecifiedEventNameSelector)tree.Statements[2];
            Assert.AreEqual("h", st2.EventName);


        }
        //        [TestMethod]
        //        public void AndGroup_AST_MultiLevelEmbededGroups()
        //        {

        //            var script =
        //@"g
        //(
        //    (
        //        a
        //        b
        //    )
        //    f
        //    (
        //        a
        //        b
        //    )
        //)
        //h";
        //            var g = new ChronExParser();
        //            var tree = g.ParsePattern(script);
        //            Assert.AreEqual(3, tree.Statements.Count);
        //            var st1 = (SpecifiedEventNameSelector)tree.Statements[0];
        //            Assert.AreEqual("g", st1.EventName);
        //            var Rootandgroup = (AndGroupElement)tree.Statements[1];
        //            Assert.AreEqual(3, Rootandgroup.Statements.Count);
        //            AssetABAndGroup((AndGroupElement)(Rootandgroup.Statements[0]));
        //            var subst1 = (SpecifiedEventNameSelector)Rootandgroup.Statements[1];
        //            Assert.AreEqual("f", subst1.EventName);
        //            AssetABAndGroup((AndGroupElement)(Rootandgroup.Statements[2]));
        //            var st2 = (SpecifiedEventNameSelector)tree.Statements[2];
        //            Assert.AreEqual("h", st2.EventName);


        //        }




        [TestMethod]
        public void OrGroup_AST_GroupWithStar()
        {

            var script =
@"[
        a
        b
     ]*"
;
            var g = new ChronExParser();
            var tree = g.ParsePattern(script);
            Assert.AreEqual(1, tree.Statements.Count);
            var st1 = (SymbolQuantifierSyntax)tree.Statements[0];
            Assert.AreEqual('*', st1.QuantifierSymbol);



            AssetABOrGroup((OrGroupElement)st1.ContainedElement);
            //var subst1 = (SpecifiedEventNameSelector)Rootandgroup.Statements[1];
            //Assert.AreEqual("f", subst1.EventName);
            //AssetABAndGroup((AndGroupElement)(Rootandgroup.Statements[2]));
            //var st2 = (SpecifiedEventNameSelector)tree.Statements[2];
            //Assert.AreEqual("h", st2.EventName);


        }
        [TestMethod]
        public void OrGroup_AST_SubEmbededInAndGroupWithNumericQuan()
        {

            var script =
@"(
    a
    [
        a
        b
    ]{1,2}
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
            AssetABOrGroup((OrGroupElement)subg.ContainedElement);

             


        }

        [TestMethod]
        public void OrGroup_SimpleGroupExactMatch()
        {
            var script =
@"[
a
b
]";
            var events = TestUtils.ChronListFromString("a b");
            var matches = ChronEx.Matches(script, events);
            matches.AssertMatchesAreEqual(
@"a
b");
}


        [TestMethod]
        public void OrGroup_SingleThenAndGroup()
        {
            var script =
@"a
[
    b
    c
    d
]";
            var events = TestUtils.ChronListFromString("a b a c a d");
            var matches = ChronEx.Matches(script, events);
            matches.AssertMatchesAreEqual(
@"a,b
a,c
a,d");
        }

        [TestMethod]
        public void OrGroup_SingleGroupSingle()
        {
            var script =
@"a
[
b
c
]
d";
            var events = TestUtils.ChronListFromString("a b d a c d");
            var matches = ChronEx.Matches(script, events);
            matches.AssertMatchesAreEqual(
@"a,b,d
a,c,d");

        }

        [TestMethod]
        public void OrGroup_2OrGroupsNestedIntoMultipAndGropus2Ors()
        {
            var script =
@"(
    [
        a
        g
    ]
    [
        b
        h
    ]
)
[
    c
    i
]
.
(
    [
        e
        k
    ]
)
.";
            var events = TestUtils.ChronListFromString("a b c d e f g h i j k l");
            var matches = ChronEx.Matches(script, events);
            matches.AssertMatchesAreEqual(
@"a,b,c,d,e,f
g,h,i,j,k,l");

        }

        [TestMethod]
        public void OrGroup_Negated()
        {
            var script =
@"a
![
    b
    c
]
d";
            var events = TestUtils.ChronListFromString("a j d a b d");
            var matches = ChronEx.Matches(script, events);
            matches.AssertMatchesAreEqual(
@"a,j,d");
 

        }



        [TestMethod]
        public void OrGroup_NumericQuantifier()
        {
            var script =
@"[
a
b
c
]{5}";
            var events = TestUtils.ChronListFromString("a b c a b a a a a a b b b b b");
            var matches = ChronEx.Matches(script, events);
            matches.AssertMatchesAreEqual(
@"a,b,c,a,b
a,a,a,a,a
b,b,b,b,b");

        }

        [TestMethod]
        public void OrGroup_StarQuantifier()
        {
            var script =
@"[
a
b
c
]*";
            var events = TestUtils.ChronListFromString("a b j a u a a b k");
            var matches = ChronEx.Matches(script, events);
            matches.AssertMatchesAreEqual(
@"a,b
a
a,a,b");

        }



        [TestMethod]
        public void OrGroup_TrailingSelectorAfterNumericGroup()
        {
            var script =
@"[
    a
    b
]{,4}
.
q";
            var events = TestUtils.ChronListFromString("a a a a b q b a a b r q");
            var matches = ChronEx.Matches(script, events);
            matches.AssertMatchesAreEqual(
@"a,a,a,a,b,q
b,a,a,b,r,q");

        }




        [TestMethod]
        public void OrGroup_MultipleConsecutiveOrGroups()
        {
            var script =
@"[
    a
    b
    c
]
[
    d
    e
    f
]
[
    g
    h
    i
]";
            var events = TestUtils.ChronListFromString("y a d g a e i a b c a c f h b c a b c");
            var matches = ChronEx.Matches(script, events);
            matches.AssertMatchesAreEqual(
@"a,d,g
a,e,i
c,f,h");

        }
        private static void AssetABOrGroup(OrGroupElement tree)
        {
            
            Assert.AreEqual(2, tree.Statements.Count);
            var it1 = (SpecifiedEventNameSelector)tree.Statements[0];
            var it2 = (SpecifiedEventNameSelector)tree.Statements[1];
            Assert.AreEqual("a", it1.EventName);
            Assert.AreEqual("b", it2.EventName);
        }
    }
}