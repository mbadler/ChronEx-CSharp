using System;
using System.Collections.Generic;
using System.Text;
using ChronEx.Models;
using ChronEx.Models.AST;

namespace ChronEx.Processor
{
    public class Tracker
    {
        private ScriptSyntaxTreeElement tree;
        IEnumerator<ElementBase> _evntEnum = null;
        public List<IChronologicalEvent> StoredList = null;
        public List<string> dbgMsgs = new List<string>();

        public bool DebugEnabled = true;
        public int dbgspaces = 0; 
        public void DebugStart(ElementBase b)
        {
            dbgMsgs.Add(" ".PadLeft(dbgspaces*2)+b.Describe());
            dbgspaces++;
        }

        public void DebugStart(ElementBase b,IChronologicalEvent evt)
        {

            dbgMsgs.Add(" ".PadLeft(dbgspaces * 2) + b.Describe()+((evt==null)?"(Null Event)":evt.Describe()));
            dbgspaces++;
        }
        public void SaveDBGResult(ElementBase b,MatchResult mr)
        {
            dbgMsgs.Add(" ".PadLeft(dbgspaces*2)+mr.ToString());
            dbgspaces--;
        }

        /// <summary>
        /// A state back passed around to elements so that we can keep the AST objects stateless
        /// </summary>
        internal Dictionary<ElementBase,object> StateBag = new Dictionary<ElementBase,object>();
        
        internal int StackDepth = 0;
        public Tracker(ScriptSyntaxTreeElement tree)
        {
            this.tree = tree;
            _evntEnum = tree.Statements.GetEnumerator();
            //element trees are guarunteed to have at lest one element 
            //so we can sfly move next
            _evntEnum.MoveNext();
        }

        internal MatchResult ProcessEvents(EventStream eventenum)
        {
            CaptureList CapturedList = new CaptureList();
            var elementenum = tree.Statements.GetEnumerator();
            //move to the first enum
            elementenum.MoveNext();
            //the enum should be set to the latest one already

            MatchResult matchres = IsMatchResult.IsNotMatch;
           
            matchres = tree.BeginProcessMatch(this,  eventenum, CapturedList);
            var CapList = CapturedList.InternalList();
            if(matchres.Is_Match() && CapList.Count>0)
            {
                this.StoredList = CapList;
            }
            return matchres;
            //var res = _evntEnum.Current.IsMatch(even,this);
            // if it does not match then we can termiante this tracker
            //return nomatch and caller will terminte

           
        }

       

        public T GetMyStateBag<T>(ElementBase Eleme) 
            {
            if(StateBag.ContainsKey(Eleme))
            {
                return (T)StateBag[Eleme];
            }
            else
            {
                return default(T);
            }
        }

        public void SetMyStateBag(ElementBase Eleme,Object StateBagObj)
        {
            StateBag[Eleme] = StateBagObj;
        }

        public void RemoveMyStateBag(ElementBase Eleme)
        {
            StateBag.Remove(Eleme);
        }



    }

    [Flags]
    public enum MatchResult
    {
        None = 0,
        Match = 1,
        Capture = 2,
        Forward = 4,
        Continue = 8,
        Ended = 16
    }
    public static class IsMatchResult
    {
        public static MatchResult IsNotMatch = MatchResult.None;
        public static MatchResult IsMatch = MatchResult.Match|MatchResult.Capture;
        
        public static bool Is_Match(this MatchResult mr)
        {
            return mr.HasFlag(MatchResult.Match);
        }
        public static bool Is_Capture(this MatchResult mr)
        {
            return mr.HasFlag(MatchResult.Capture);
        }
        public static bool Is_Forward(this MatchResult mr)
        {
            return mr.HasFlag(MatchResult.Forward);
        }
        public static bool Is_Continue(this MatchResult mr)
        {
            return mr.HasFlag(MatchResult.Continue);
        }

        public static bool Is_Ended(this MatchResult mr)
        {
            return mr.HasFlag(MatchResult.Ended);
        }
    }
}
