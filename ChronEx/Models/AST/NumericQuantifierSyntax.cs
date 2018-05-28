using System;
using System.Collections.Generic;
using System.Text;
using ChronEx.Parser;
using ChronEx.Processor;
using System.Diagnostics;

namespace ChronEx.Models.AST
{
    [DebuggerDisplayAttribute("NumericQuantifierSyntax: MinOccours = {MinOccours} , MaxOccours = {MaxOccours}")]
    public class NumericQuantifierSyntax : QuantifierElementBase
    {
        public override int ZOrder => 500;

        public override string Describe()
        {
            return $"NumericQuantifierSyntax: MinOccours = {MinOccours} , MaxOccours = {MaxOccours}";
        }
        // we will handle transtions internally so we don't need to specify actual trnsition records
        static Dictionary<StatementState, AllowedTransition> MyTransitions = new Dictionary<StatementState, AllowedTransition>()
        {
            {StatementState.NumericQuantifierStart,new AllowedTransition()
                {
                    {LexedTokenType.NUMBER,TransitionRecord.Blank() },
                    {LexedTokenType.COMMA,TransitionRecord.Blank() }
                }
            },
            {StatementState.NumericQuantifierMin,new AllowedTransition()
                {
                    {LexedTokenType.COMMA,TransitionRecord.Blank() },
                    {LexedTokenType.CLOSECURLY,TransitionRecord.Blank() }
                }
            },
            {StatementState.NumericQuantifierComma,new AllowedTransition()
                {
                    {LexedTokenType.CLOSECURLY,TransitionRecord.Blank() },
                    {LexedTokenType.NUMBER,TransitionRecord.Blank() }
                }
            },
            {StatementState.NumericQuantifierMax,new AllowedTransition()
                {
                    {LexedTokenType.CLOSECURLY,TransitionRecord.Blank() }
                }
            }



        };

        public int MinOccours { get; set; } = 0;
        public int MaxOccours { get; set; } = int.MaxValue;

        public override void InitializeFromParseStream(ParseProcessState state)
        {
            while (state.MoveNext(MyTransitions[state.State],false) != null)
            {
                var curr = state.Current().Value;
                switch (state.State)
                {

                    case StatementState.NumericQuantifierStart:
                        {
                            if(curr.TokenType==LexedTokenType.NUMBER)
                            {
                                if(state.Peek(1).Value.TokenType==LexedTokenType.CLOSECURLY)
                                {
                                    var parseval = int.Parse(curr.TokenText);
                                    this.MinOccours = parseval;
                                    this.MaxOccours = parseval;
                                    state.State = StatementState.NumericQuantifierMax;
                                    break;
                                }
                                MinOccours = int.Parse(curr.TokenText);
                                state.State = StatementState.NumericQuantifierMin;
                            }
                            else
                            {
                                state.State = StatementState.NumericQuantifierComma;
                            }
                            break;
                        }
                        
                    case StatementState.NumericQuantifierMin:
                        {
                            state.State = StatementState.NumericQuantifierComma;
                            break;
                        }
                    case StatementState.NumericQuantifierComma:
                        {
                            if (curr.TokenType == LexedTokenType.NUMBER)
                            {
                                MaxOccours = int.Parse(curr.TokenText);
                                state.State = StatementState.NumericQuantifierMax;
                            }
                            else
                            {
                                state.State = StatementState.NumericQuantifierEnd;
                                return;
                            }
                            break;
                        }
                    case StatementState.NumericQuantifierMax:
                        {
                            state.State = StatementState.NumericQuantifierEnd;
                            return;
                        }
                    case StatementState.NumericQuantifierEnd:
                        return;

                    default:
                        throw new Exception("Invalid State '" + state.State.ToString() + " in NumericQuantifier , this is caused by an internal bug");
                }
            }
        }

        internal override MatchResult SubBeginProcessMatch(Tracker tracker, IEventStream eventenum, CaptureList CapturedList)
        {
            if (eventenum.Current == null)
            {
                return MatchResult.None;
            }
                MatchResult tres = MatchResult.None;
            var sbag = GetQuantifierState(tracker);
            if (!sbag.MatchCount.HasValue)
            {
                sbag.MatchCount = new Nullable<int>(0);
                tracker.SetMyStateBag(this, sbag);
            }
            var specEventStream = eventenum;//eventenum.CreateSpeculator();
            do
            {
                MatchResult subRes = MatchResult.None;
                
                subRes = ContainedElement.BeginProcessMatch(tracker, eventenum, sbag.CaptureList);
                if (!subRes.Is_Match())
                {
                    //if we matched our minimun we can make a final decisison if it did not match
                    if (sbag.MatchCount.Value >= MinOccours)
                    {
                        tres = MatchResult.Match | MatchResult.Forward;
                    }
                    else//if we have not yet matched our minimun then we just need to continue
                    {
                        tres = MatchResult.Continue | MatchResult.Capture;

                    }



                }
                

                //if we did match
                if (subRes.Is_Match())
                {
                    //if we reached the max then return a match
                    if (sbag.MatchCount.Value + 1 == MaxOccours)
                    {
                        tres = IsMatchResult.IsMatch;
                    }
                    else
                    {
                        //if we matched at least min then mark as match and contineu
                        if (sbag.MatchCount.Value +1 >= MinOccours)
                        {
                            tres = MatchResult.Match | MatchResult.Continue | MatchResult.Capture;
                        }
                        else
                        {
                            //just continue matching
                            //if the subres has a forward , then don't capture , just relay the forward
                            if(subRes.Is_Forward())
                            {
                                tres = MatchResult.Continue | MatchResult.Forward;
                            }
                            else
                            {
                            tres = MatchResult.Continue | MatchResult.Capture;
                            }
                        }
                    }

                }

                //if its the end (either a match or not) , then remove our bag state
                if (!tres.Is_Continue())
                {
                    tracker.RemoveMyStateBag(this);
                }
                else
                {
                    //if its a continue then increment the bagstate
                    sbag.MatchCount++;
                    tracker.SetMyStateBag(this, sbag);
                }

                if (subRes.Is_Forward())
                {
                    tres = tres | MatchResult.Forward;
                }

                //if this is a match without a continue then grab the commmit the speculator
                //and add the spculative captures to the tracker captures
                if (tres.Is_Match() && !tres.Is_Continue())
                {
                    //specEventStream.EndApplyAll();
                    if (CapturedList != null)
                    {
                        CapturedList.AddRange(sbag.CaptureList.Items);
                    }
                    return tres;
                }

                //if this is not a match and not a continue then rollback the speculator and return the result
                if (!tres.Is_Match() && !tres.Is_Continue())
                {
                    //specEventStream.EndDiscardAll();
                    return tres;
                }
                //its a continue so allow the loop to continue

            }
            while (
            specEventStream.MoveNextIfNotForward(tres)); 
            //if we are here it means that we ran out of events so we got to deal with that
           if (tres.Is_Match())
            {
                if (CapturedList != null)
                {
                    CapturedList.AddRange(sbag.CaptureList.Items);
                }
            }
            //remove the continue if its there
            if (tres.Is_Continue())
            {
                var mask = ~MatchResult.Continue;
                tres = tres & mask;
            }
            return tres;
        }

        //internal override MatchResult IsMatch(IChronologicalEvent chronevent, Tracker Tracker, List<IChronologicalEvent> CapturedList)
        //{
            

        //    //Pass the event along to the contained element and get its result
        //    var subRes = ContainedElement.IsMatch(chronevent, Tracker, sbag.CaptureList);
        //    MatchResult tres = IsMatchResult.IsNotMatch;

        //    //if the chiled element did not match , then check if we reachied the min
            

        //}

       

        internal override bool IsPotentialMatch(IChronologicalEvent chronevent)
        {
            //too complicated to try to predict just return true
            return true;
        }

        internal override MatchResult IsMatch(IChronologicalEvent chronevent, Tracker Tracker, CaptureList CapturedList)
        {
            throw new NotImplementedException();
        }
    }

    public class QuantifierState
    {
        public int? MatchCount { get; set; }
        public CaptureList CaptureList { get; set; } = new CaptureList();
    }
}
