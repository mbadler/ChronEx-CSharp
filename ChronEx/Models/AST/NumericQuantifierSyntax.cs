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
            while (state.MoveNext(MyTransitions[state.State]) != null)
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

        internal override MatchResult IsMatch(IChronologicalEvent chronevent, Tracker Tracker, List<IChronologicalEvent> CapturedList)
        {
            //first lets check the statebag to see if we are in there yet
            var sbag = Tracker.GetMyStateBag<int?>(this);
            if (!sbag.HasValue)
            {
                sbag = new Nullable<int>(0);
                Tracker.SetMyStateBag(this, sbag);
            }

            //Pass the event along to the contained element and get its result
            var subRes = ContainedElement.IsMatch(chronevent, Tracker, CapturedList);
            MatchResult tres = IsMatchResult.IsNotMatch;
            
            //if the chiled element did not match , then check if we reachied the min
            if(subRes==IsMatchResult.IsNotMatch)
            {
                if(sbag.Value >= MinOccours)
                {
                    tres = MatchResult.Match | MatchResult.Forward;
                }
                else
                {
                    tres = IsMatchResult.IsNotMatch;
                }
            }

            //if we did match
            if(subRes==IsMatchResult.IsMatch)
            {
                //if we reached the max then return a match
                if(sbag.Value +1 == MaxOccours)
                {
                    tres = MatchResult.Match;
                }else
                //if we are still under the max then continue matching 
                tres = MatchResult.Continue;
            }

            //if its the end (either a match or not) , then remove our bag state
            if (tres == IsMatchResult.IsNotMatch || tres.HasFlag(MatchResult.Forward) || tres.HasFlag(MatchResult.Match))
            {
                Tracker.RemoveMyStateBag(this);
            }
            else
            {
                //if its a continue then increment the bagstate
                sbag = sbag.Value + 1;
                Tracker.SetMyStateBag(this, sbag);
            }

            //return to the tracker the result
            return tres;

        }

        internal override bool IsPotentialMatch(IChronologicalEvent chronevent)
        {
            //too complicated to try to predict just return true
            return true;
        }
    }
}
