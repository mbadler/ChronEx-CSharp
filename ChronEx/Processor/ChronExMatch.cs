using ChronEx.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace ChronEx.Processor
{
    [DebuggerDisplay("Count = {CapturedEvents.Count}")]
    public class ChronExMatch
    {
        public List<IChronologicalEvent> CapturedEvents { get; set; }
    }
}
