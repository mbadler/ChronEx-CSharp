using System;
using System.Collections.Generic;
using System.Text;
using ChronEx.Parser;
using ChronEx.Processor;

namespace ChronEx.Models.AST
{
    public class ScriptSyntaxTreeElement : StatementContainerElement
    {
        public override int ZOrder => 0;

        

        internal override IsMatchResult IsMatch(IChronologicalEvent chronevent, Tracker Tracker)
        {
            throw new NotImplementedException();
        }

        internal override bool IsPotentialMatch(IChronologicalEvent chronevent)
        {
            throw new NotImplementedException();
        }
    }
}
