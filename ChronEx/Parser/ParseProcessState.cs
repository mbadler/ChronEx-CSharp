using ChronEx.Models.AST;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace ChronEx.Parser
{
   public class ParseProcessState
    {

        public StatementState State { get; set; }
        public List<LexedToken> Tokens { get; set; }
        public int CurrentIndex { get; set; } = -1;
        private AllowedTransition CurrentTransition { get; set; }

        public ElementBase CreateCurrent()
        {
            var FactoryName = CurrentTransition[Current().Value.TokenType].Factory;
            var con = ParseStates.Constructors[FactoryName];
            State = con.Item1;
            //some tokens don't need any kind of constructig exit if this is them
            if(con.Item2 == null)
            {
                return null;
            }

            //create the element
            var newElement =con.Item2();
            //recurse the stream into the new element if it needs to do anything
            newElement.InitializeFromParseStream(this);
            return newElement;
        }

        public LexedToken? MoveNext()
        {
            if(CurrentIndex+1 == Tokens.Count)
            {
                return null;
            }
            else
            {
                CurrentIndex++;
                var curTok = Tokens[CurrentIndex];
                var TransitionRecord = ParseStates.StateTransitions[State];
                if(!TransitionRecord.ContainsKey(curTok.TokenType))
                {
                    throw new ParserStateException(State, curTok, TransitionRecord);
                }
                CurrentTransition = TransitionRecord;
                return curTok;
            }
        }

        public LexedToken? Peek(int aheadCount)
        {
            if (CurrentIndex +aheadCount > Tokens.Count
                ||Tokens.Count  < CurrentIndex+aheadCount)
            {
                return null;
            }
            else
            {

                return Tokens[CurrentIndex +aheadCount] ;
            }
        }

        public LexedToken? Current()
        {
            if (CurrentIndex> Tokens.Count
                || Tokens.Count < CurrentIndex)
            {
                return null;
            }
            else
            {

                return Tokens[CurrentIndex];
            }
        }


    }

    [Serializable]
    internal class ParserStateException : Exception
    {
        private StatementState state;
        private LexedToken curTok;
        private AllowedTransition transitionRecord;

        public ParserStateException()
        {
        }

        public ParserStateException(string message) : base(message)
        {
        }

        public ParserStateException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public ParserStateException(StatementState state, LexedToken curTok, AllowedTransition transitionRecord) :
            base($"Invalid token {curTok.TokenType.ToString()} in {state.ToString()}")
        {
            this.state = state;
            this.curTok = curTok;
            this.transitionRecord = transitionRecord;
        }

        protected ParserStateException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
