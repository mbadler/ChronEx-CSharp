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

        public override void InitializeFromParseStream(ParseProcessState state)
        {
            //nothing really to do here , negation is negation
        }

       

        internal override IsMatchResult IsMatch(IChronologicalEvent chronevent, Tracker Tracker)
        {
            switch (ContainedElement.IsMatch(chronevent, Tracker))
            {
                case Processor.IsMatchResult.IsMatch:
                    {
                        return Processor.IsMatchResult.IsNotMatch;
                         
                    }
                case Processor.IsMatchResult.IsNotMatch:
                    {
                        return Processor.IsMatchResult.IsMatch;
                    }

                case Processor.IsMatchResult.Continue:
                    {
                        return Processor.IsMatchResult.Continue;
                    }
                default:
                    return Processor.IsMatchResult.Continue;
            }
        }

        

        internal override bool IsPotentialMatch(IChronologicalEvent chronevent)
        {
            return !ContainedElement.IsPotentialMatch(chronevent);
        }
    }
}
