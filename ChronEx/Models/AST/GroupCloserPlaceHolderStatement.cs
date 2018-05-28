using System;
using System.Collections.Generic;
using System.Text;
using ChronEx.Parser;
using ChronEx.Processor;

namespace ChronEx.Models.AST
{
    public class GroupCloserPlaceHolderElement : ElementBase
    {
        public override int ZOrder => throw new NotImplementedException();

        public override string Describe()
        {
            throw new NotImplementedException();
        }

        public override void InitializeFromParseStream(ParseProcessState state)
        {
            //eat up the transition token
            //state.MoveNext();
        }

        internal override MatchResult IsMatch(IChronologicalEvent chronevent, Tracker Tracker, CaptureList CapturedList)
        {
            throw new NotImplementedException();
        }

        internal override bool IsPotentialMatch(IChronologicalEvent chronevent)
        {
            throw new NotImplementedException();
        }
    }
}
