using System;
using System.Collections.Generic;
using System.Text;
using ChronEx.Parser;
using ChronEx.Processor;

namespace ChronEx.Models.AST
{
    /// <summary>
    /// - Syntax indicates that result from child matches should not be captured
    /// </summary>
    public class NoCaptureSyntax : ContainerElement
    {
        //Currently the highest zorder
        public override int ZOrder => 1000;

        public override void InitializeFromParseStream(ParseProcessState state)
        {
            //nothing to do
        }

        public override string Describe()
        {
            return "No Capture";
        }

        internal override MatchResult IsMatch(IChronologicalEvent chronevent, Tracker Tracker, CaptureList CapturedList)
        {
            var a = ContainedElement.IsMatch(chronevent, Tracker, CapturedList);
            
            return a; 
           
        }


        internal override MatchResult BeginProcessMatch(Tracker tracker, IEventStream eventenum, CaptureList CapturedList)
        {
            tracker.DebugStart(this);
            //we don't want to capture so send in a null
            var r = ContainedElement.BeginProcessMatch(tracker, eventenum, null);
            if(tracker.DebugEnabled)
            {
                tracker.SaveDBGResult(this, r);
            }
            return r;
        }
        internal override bool IsPotentialMatch(IChronologicalEvent chronevent)
        {
            //just forward to the child 
            return ContainedElement.IsPotentialMatch(chronevent);
        }
    }
}
