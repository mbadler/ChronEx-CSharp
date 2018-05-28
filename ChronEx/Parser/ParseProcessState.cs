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
            string FactoryName = "";
            try
            {
FactoryName = CurrentTransition[Current().Value.TokenType].Factory;
            }
            catch (Exception ex)
            {
                var d = ex;
            }
            //if this is a terminating state (such as a group)
            if(FactoryName=="")
            {
                return null;
            }
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

        /// <summary>
        /// Moves to the next token , additionally ensures that the token transition is valid
        /// generally looks at the global token tree , but subclasses can provide thier own allowed transitions to provide dynamic , localized validation
        /// </summary>
        /// <param name="tr"></param>
        /// <returns></returns>
        public LexedToken? MoveNext(AllowedTransition tr, bool SkipWhitespace)
        {
            if (CurrentIndex + 1 == Tokens.Count)
            {
                return null;
            }

            CurrentIndex++;
            var curTok = Tokens[CurrentIndex];

            if (SkipWhitespace) 
            {
                while (curTok.TokenType == LexedTokenType.WHITESPACE)
                {
                    if (CurrentIndex + 1 == Tokens.Count)
                    {
                        return null;
                    }
                    CurrentIndex++;
                    curTok = Tokens[CurrentIndex];
                }
                
            }

            var TransitionRecord = tr;
            if (tr == null)
            {
                TransitionRecord = ParseStates.StateTransitions[State];
            }
            if (!TransitionRecord.ContainsKey(curTok.TokenType))
            {
                throw new ParserStateException(State, curTok, TransitionRecord);
            }
            CurrentTransition = TransitionRecord;
            return curTok;
        }
        

        public LexedToken? MoveNext()
        {
            return MoveNext(null,false);
        }

        public LexedToken? MoveNext(bool SkipWhitespace)
        {
            return MoveNext(null, SkipWhitespace);
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
