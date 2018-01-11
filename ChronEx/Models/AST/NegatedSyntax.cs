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

       

        internal override MatchResult IsMatch(IChronologicalEvent chronevent, Tracker Tracker, List<IChronologicalEvent> CapturedList)
        {
            //can't neagte a null - if null event then return no match
            if(chronevent == null)
            {
               
                return IsMatchResult.IsNotMatch;
            }
            var a = ContainedElement.IsMatch(chronevent, Tracker, CapturedList);
            if (a.Is_Match())
                {
                a= IsMatchResult.IsNotMatch;
            }
            else
            {
                a= IsMatchResult.IsMatch;
            }
           
            return a;

        }

        

        internal override bool IsPotentialMatch(IChronologicalEvent chronevent)
        {
            return !ContainedElement.IsPotentialMatch(chronevent);
        }
    }
}
