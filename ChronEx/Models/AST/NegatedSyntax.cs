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

        //public override ElementBase ReturnParseTreeFromExistingElement(ElementBase ExisitngElement)
        //{
        //    //i am usually the top of the stack , except if its a DashSyntax
        //    // if there is no existign the just return me
        //    if (ExisitngElement == null)
        //    {
        //        return this;
        //    }

        //    // if the existing element is a dash syntax then add myseld to it
        //    if(ExisitngElement is NoCaptureSyntax)
        //    {
        //        ((NoCaptureSyntax)ExisitngElement).AddContainedElement(this);
        //        return ExisitngElement;
        //    }

        //    this.AddContainedElement(ExisitngElement);
        //    return this;
        //}

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
