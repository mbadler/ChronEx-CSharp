using System;
using System.Collections.Generic;
using System.Text;
using ChronEx.Processor;

namespace ChronEx.Models.AST
{
    public class OrGroupElement : StatementContainerElement
    {
        public override int ZOrder => 0;

        public override string Describe()
        {
            return "OrGroup";
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
            foreach (var item in Statements)
            {
                var r = item.BeginProcessMatch(tracker, eventenum, CapturedList);
                if(r.Is_Match())
                {
                    return r;
                }

            }
            return MatchResult.None;
        }
    }
}
