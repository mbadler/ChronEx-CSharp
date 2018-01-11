using ChronEx.Models;
using ChronEx.Models.AST;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace ChronEx.Processor
{

    public class Runner
    {
        protected ScriptSyntaxTreeElement tree;

        protected IEnumerable<IChronologicalEvent> EventList { get; }

        protected List<Tracker> trackList = new List<Tracker>();
        /// <summary>
        /// Trackers that were found to have results and the captures
        /// </summary>
        protected List<Tracker> CapturedtrackList = new List<Tracker>();

        protected int numOfMatches = 0;

        // for eventual streaming implementation
        public Runner()
        {

        }

        public Runner(ScriptSyntaxTreeElement tree, IEnumerable<IChronologicalEvent> EventList)
        {
            this.tree = tree;
            this.EventList = EventList;
        }

        //runs thru the events and returns a bool if the pattern matches the events
        //Currently "Event Directed Engine" (conceptually a Text directed Engine in regex world)
        //For the future we can look at creating a (Pattern Directed Engine)
        public virtual bool IsMatch()
        {
            return PerformMatch(true, false).Item2 > 0;
        }

        //runs thru the events and returns the number of matches found
        public virtual int MatchCount()
        {
            return PerformMatch(false, false).Item2;
        }

        /// <summary>
        /// Returns a list containing the list of matches found
        /// </summary>
        /// <returns></returns>
        public virtual ChronExMatches Matches()
        {
            return PerformMatch(false, true).Item1;
        }

        /// <summary>
        /// Performs pattern matching
        /// </summary>
        /// <param name="ShortCircuit">Indicates to return as soon as a match is found</param>
        /// <returns>if short circuit 1 for match 0 for no matches , if not short circuit will return the number of matches found</returns>
        protected (ChronExMatches, int) PerformMatch(bool ShortCircuit, bool Store)
        {
            return PerformMatch(ShortCircuit, Store, ChronExMatchOptions.Default);
        }

        /// <summary>
        /// Performs pattern matching
        /// </summary>
        /// <param name="ShortCircuit">Indicates to return as soon as a match is found</param>
        /// <returns>if short circuit 1 for match 0 for no matches , if not short circuit will return the number of matches found</returns>
        protected (ChronExMatches, int) PerformMatch(bool ShortCircuit, bool Store, ChronExMatchOptions options)
        {
            var debugTrackers = new List<Tracker>();
            // get the first element this is the lowest level filter to launching a tracker
            var getlemes = tree.Statements;
            if (!getlemes.Any())
            {
                return (null, 0);
            }
            var firstElem = getlemes.First();
            List<Tracker> FoundTrackers = new List<Tracker>();
            Tracker CurrentTracker = null;
            var eventenum = EventList.GetEnumerator();
            eventenum.MoveNext();
            while (eventenum.Current!=null)
            {
                //if we don't have a tracker yet then determine if this event will possible start one
                if (CurrentTracker == null)
                {
                    if (firstElem.IsPotentialMatch(eventenum.Current))
                    {
                        CurrentTracker = new Tracker(tree);
                    }
                    else
                    {
                        //useually the tracker and the elemtn would move the line forward
                        //but since there is not tracker and there is no potential match
                        //the runner needs to move it forward
                        eventenum.MoveNext();
                        continue;
                    }
                }
                var res = CurrentTracker.ProcessEvents(eventenum);
                if (res.Is_Match())
                {
                    FoundTrackers.Add(CurrentTracker);
                }
                //else
                //{
                //    //we need to manually move the  the event
                //    eventenum.MoveNext();
                //}
                if(CurrentTracker!=null)
                {
                    debugTrackers.Add(CurrentTracker);
                }
                CurrentTracker = null;
            }

            var lex = new ChronExMatches();
            foreach (var item in FoundTrackers)
            {
                var cmatch = new ChronExMatch();
                cmatch.CapturedEvents = item.StoredList;
                lex.Add(cmatch);
            }
            lex.DebugTrackers = debugTrackers;
            return (lex, lex.Count);


        }
      



        
    }
}
