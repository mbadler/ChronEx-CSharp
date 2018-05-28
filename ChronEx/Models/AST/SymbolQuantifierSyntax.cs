using System;
using System.Collections.Generic;
using System.Text;
using ChronEx.Parser;
using ChronEx.Processor;
using System.Diagnostics;

namespace ChronEx.Models.AST
{
    [DebuggerDisplayAttribute("SymbolQuantifierSyntax: QuantifierSymbol = {QuantifierSymbol}")]
    public class SymbolQuantifierSyntax : QuantifierElementBase
    {

        public char QuantifierSymbol { get; set; }

        //very high in the zorder list
        public override int ZOrder => 500;


        public override string Describe()
        {
            return $"SymbolQuantifierSyntax: QuantifierSymbol = {QuantifierSymbol}";
        }
        //public override ElementBase ReturnParseTreeFromExistingElement(ElementBase ExisitngElement)
        //{
        //    //for now I am the top element inthe chain so add it to me and return myself
        //    AddContainedElement(ExisitngElement);
        //    return this;
        //}

        internal override MatchResult SubBeginProcessMatch(Tracker tracker, IEventStream eventenum, CaptureList CapturedList)
        {
            //first lets check the statebag to see if we are in there yet
            var sbag = GetQuantifierState(tracker);
            if (!sbag.MatchCount.HasValue)
            {
                sbag.MatchCount = new Nullable<int>(0);
                tracker.SetMyStateBag(this, sbag);
            }

            //Pass the event along to the contained element and get its result
            var subRes = ContainedElement.BeginProcessMatch(tracker,eventenum, sbag.CaptureList);

            MatchResult tres = IsMatchResult.IsNotMatch;
            if (QuantifierSymbol == '+')
            {
                tres = HandlePlus(sbag.MatchCount, subRes);
            }

            if (QuantifierSymbol == '*')
            {
                tres = HandleStar(sbag.MatchCount, subRes);
            }

            if (QuantifierSymbol == '?')
            {
                tres = HandleQuestionMark(sbag.MatchCount, subRes);
            }

            //if its the end (either a match or not) , then remove our bag state
            if (!tres.Is_Continue())
            {
                if (tres.Is_Match() )
                {
                    //specEventStream.EndApplyAll();
                    if (CapturedList != null)
                    {
                        CapturedList.AddRange(sbag.CaptureList.Items);
                    }
                    
                }
                tracker.RemoveMyStateBag(this);
            }
            else
            {
                //if its a continue then increment the bagstate
                sbag.MatchCount = sbag.MatchCount.Value + 1;
                tracker.SetMyStateBag(this, sbag);
            }

            
            
            //return to the tracker the result
            if(subRes.Is_Forward())
            {
                tres = tres | MatchResult.Forward;
            }
            return tres;
        }
        internal override MatchResult IsMatch(IChronologicalEvent chronevent, Tracker Tracker, CaptureList CapturedList)
        {

            throw new NotImplementedException();
        }

        private MatchResult HandleQuestionMark(int? sbag, MatchResult subRes)
        {
            //rules for star
            //Matches 0 or 1 events, if there are any events then they are captured if there aren't any then nothing s captured
            //question mark is the simpelst we don't need any major logic

            //if it matches then capture
            if (subRes.Is_Match())
            {
                return MatchResult.Match|MatchResult.Capture;
            }

            //if it does not match then return a non matching forward and don't capture
            if (!subRes.Is_Match())
            {
                return MatchResult.Forward|MatchResult.Match;
               
            }

            throw new Exception("Unexpected");

        }

        private MatchResult HandleStar(int? sbag, MatchResult subRes)
        {
            //rules for star
            //Matches 0 or more events, if there are any events then they are captured if there aren't any then nothing s captured

            //lets first handle the simpilest scenerio - no match at all
            //return a forward and no capture
            if (sbag.Value == 0 && !subRes.Is_Match())
            {
                return MatchResult.Match | MatchResult.Forward;
               // return (Processor.MatchResult.ForwardToNextYesMatch,false);
            }

            //next scenerio - this is either the first or subsequent got arounds and we match
            //return a continue and capture
            if (subRes.Is_Match())
            {
                return MatchResult.Continue | MatchResult.Match|MatchResult.Capture;
            }

            //next scenerio - we matched previously but we dont match now
            // we mark ourself as sucessful but we don't capture this element we pass it on to the next
            if (sbag.Value > 0 && !subRes.Is_Match())
            {
                return MatchResult.Match | MatchResult.Forward;
            }

            //for now should not be any other possible scenrio
            throw new Exception("Unexpected");
        }

        private MatchResult HandlePlus(int? sbag, MatchResult subRes)
        {
            //rules for plus
            //Matches at least 1 event but will capture all matching events

            //lets first handle the simpilest scenerio - nothing found yet and this is not a match
            //return a no forward no matchmatch, do not capture
            if(sbag.Value == 0 && !subRes.Is_Match())
            {
                return Processor.MatchResult.Forward;
            }

            //next scenerio - this is either the first or subsequent got arounds and we match
            if(subRes.Is_Match())
            {
                return MatchResult.Match | MatchResult.Continue|MatchResult.Capture;
            }

            //next scenerio - we matched previously but we dont match now
            // we mark ourself as sucessful but we don't capture this element we pass it on to the next
            if(sbag.Value > 0 && !subRes.Is_Match())
            {
                return MatchResult.Forward | MatchResult.Match;
            }

            //for now should not be any other possible scenrio
            throw new Exception("Unexpected");
        }



        //for now return true;
        internal override bool IsPotentialMatch(IChronologicalEvent chronevent)
        {
            return true;
        }

        public override void InitializeFromParseStream(ParseProcessState state)
        {
            switch (state.Current().Value.TokenType)
            {
                
                case LexedTokenType.PLUS:
                    {
                        this.QuantifierSymbol = '+';
                        break;
                    }
                case LexedTokenType.STAR:
                    {
                        this.QuantifierSymbol = '*';
                        break;
                    }
                case LexedTokenType.QUESTIONMARK:
                    {
                        this.QuantifierSymbol = '?';
                        break;
                    }

                default:
                    throw new Exception($"Internal Error: A non valid symbol '{state.Current().Value.TokenType.ToString()}' was passed as SymbolQuantifier. This is a bug");
            }
        }
    }
}
