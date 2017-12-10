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

        internal override IsMatchResult IsMatch(IChronologicalEvent chronevent, Tracker Tracker)
        {
            var res = ContainedElement.IsMatch(chronevent, Tracker);
            if (res == IsMatchResult.IsMatch)
            {
                return IsMatchResult.IsMatch_NoCapture;
            }
            if(res== IsMatchResult.Continue)
            {
                return IsMatchResult.Continue_NoCapture;
            }
            return res;
        }

        internal override bool IsPotentialMatch(IChronologicalEvent chronevent)
        {
            //just forward to the child 
            return ContainedElement.IsPotentialMatch(chronevent);
        }
    }
}
