using ChronEx.Parser;
using System;
using System.Collections.Generic;
using System.Text;
using ChronEx.Processor;

namespace ChronEx.Models.AST
{
    public abstract class QuantifierElementBase:ContainerElement
    {
        internal override MatchResult BeginProcessMatch(Tracker tracker, EventStream eventenum, List<IChronologicalEvent> CapturedList)
        {
            tracker.DebugStart(this,eventenum.Current);
            // we need to override this becasue the base class does event capturing
            // we handle capturing internally
            var b = this.IsMatch(eventenum.Current, tracker, CapturedList);
            
            if (tracker.DebugEnabled)
            {
                tracker.SaveDBGResult(this, b);
            }
            return b;


        }
    }
}
