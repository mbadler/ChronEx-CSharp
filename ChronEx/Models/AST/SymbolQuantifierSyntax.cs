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

        internal override MatchResult IsMatch(IChronologicalEvent chronevent, Tracker Tracker, List<IChronologicalEvent> CapturedList)
        {
            //first lets check the statebag to see if we are in there yet
            var sbag = Tracker.GetMyStateBag<int?>(this);
            if (!sbag.HasValue)
            {
                sbag = new Nullable<int>(0);
                Tracker.SetMyStateBag(this,sbag);
            }

            //Pass the event along to the contained element and get its result
            var subRes = ContainedElement.IsMatch(chronevent, Tracker, CapturedList);

            MatchResult tres = IsMatchResult.IsNotMatch;
            if(QuantifierSymbol=='+')
            {
                tres = HandlePlus(sbag, subRes);
            }

            if (QuantifierSymbol == '*')
            {
                tres = HandleStar(sbag, subRes);
            }

            if (QuantifierSymbol == '?')
            {
                tres = HandleQuestionMark(sbag, subRes);
            }

            //if its the end (either a match or not) , then remove our bag state
            if (!tres.Is_Continue())
            {
                Tracker.RemoveMyStateBag(this);
            }
            else
            {
                //if its a continue then increment the bagstate
                sbag = sbag.Value+1;
                Tracker.SetMyStateBag(this, sbag);
            }

            if(tres.Is_Capture() && CapturedList!=null)
            {
                CapturedList.Add(chronevent);
            }
            //return to the tracker the result
           
            return tres;

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
