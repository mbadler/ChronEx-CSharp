using ChronEx.Parser;
using System;
using System.Collections.Generic;
using System.Text;
using ChronEx.Processor;

namespace ChronEx.Models.AST
{
    public abstract class QuantifierElementBase:ContainerElement
    {
        internal override MatchResult BeginProcessMatch(Tracker tracker, IEventStream eventenum, CaptureList CapturedList)
        {
            tracker.DebugStart(this, eventenum.Current);
            // we need to override this becasue the base class does event capturing
            // we handle capturing internally
            var b = this.SubBeginProcessMatch(tracker, eventenum, CapturedList);

            if (tracker.DebugEnabled)
            {
                tracker.SaveDBGResult(this, b);
            }
            return b;


        }

        protected QuantifierState GetQuantifierState(Tracker Tracker)
        {
            //first lets check the statebag to see if we are in there yet
            if (Tracker.StateBag.ContainsKey(this))
            {
                return (QuantifierState)Tracker.StateBag[this];
            }
            else
            {
                return new QuantifierState();
            }
        }
        internal abstract MatchResult SubBeginProcessMatch(Tracker tracker, IEventStream eventenum, CaptureList capturedList);
    }
}
