using System;
using System.Collections.Generic;
using System.Text;

namespace ChronEx.Processor
{
    public class ChronExMatchOptions
    {
        public static readonly ChronExMatchOptions Default = new ChronExMatchOptions();

        public MatchTrackingMode MatchTrackingMode { get; set; } = MatchTrackingMode.Standard;
    }

    public enum MatchTrackingMode
    {
        /// <summary>
        /// Standard match tracking captures are not reused for other potential matches
        /// Example: given a input of "aaaa" and a statement of a{2} you will get 2 matches of aa
        /// </summary>
        Standard,
        /// <summary>
        /// Every element can be a starting point for a match so that elements can be reused
        /// Example: given a input of "aaaa" and a statement of a{2} you will get 3 matches of aa
        /// </summary>
        Comprehensive
    }

}
