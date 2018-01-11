using System;
using System.Collections.Generic;
using System.Text;

namespace ChronEx.Processor
{
    public class ChronExMatches : List<ChronExMatch>
    {
        public List<Tracker> DebugTrackers { get; internal set; }
    }
}
 