using ChronEx.Parser;
using ChronEx.Processor;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChronEx.Models.AST
{
    public class NegatedSyntax: ContainerElement
    {
        public override int ZOrder => 100;

        public override string Describe()
        {
            return "Negation";
        }

        public override void InitializeFromParseStream(ParseProcessState state)
        {
            //nothing really to do here , negation is negation
        }


        internal override MatchResult BeginProcessMatch(Tracker tracker, IEventStream eventenum, CaptureList CapturedList)
        {

            //can't neagte a null - if null event then return no match
            if (eventenum.Current == null)
            {

                return IsMatchResult.IsNotMatch;
            }
             
            var a = ContainedElement.BeginProcessMatch(tracker, eventenum, null);
            if (a.Is_Match())
            {
                a = IsMatchResult.IsNotMatch;
                
                
            }
            else
            {CapturedList.Add(eventenum.Current);
                a = IsMatchResult.IsMatch;
                
            }

            return a;
        }

        internal override MatchResult IsMatch(IChronologicalEvent chronevent, Tracker Tracker, CaptureList CapturedList)
        {
            throw new NotImplementedException();
        }

        internal override bool IsPotentialMatch(IChronologicalEvent chronevent)
        {
            return !ContainedElement.IsPotentialMatch(chronevent);
        }
    }
}
