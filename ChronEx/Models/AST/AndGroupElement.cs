using System;
using System.Collections.Generic;
using System.Text;
using ChronEx.Processor;

namespace ChronEx.Models.AST
{
    public class AndGroupElement : StatementContainerElement
    {
        public override int ZOrder => 0;

        public override string Describe()
        {
            return "AndGroup";
        }

        internal override MatchResult IsMatch(IChronologicalEvent chronevent, Tracker Tracker, CaptureList CapturedList)
        {
            //should not be called , processed by the prcoessmatfhes function
            throw new NotImplementedException();
        }

        internal override bool IsPotentialMatch(IChronologicalEvent chronevent)
        {
            return true;
        }

        internal override MatchResult BeginProcessMatch(Tracker tracker, IEventStream eventenum, CaptureList CapturedList)
        {
            //since the loop has already advanced the event for the next item , also mark it as a forward so 
            //that when the parent loop continues processing it should resuse the current item
            //var spec = eventenum.CreateSpeculator();
            var g = base.BeginProcessMatch(tracker, eventenum, CapturedList)| MatchResult.Forward ;
            //if (!g.Is_Ended())
            //{
            //    g = g   ;
            //}
            //else
            //{
            //    spec.EndDiscardAll();
            //}
            return g;
        }
    }
}
