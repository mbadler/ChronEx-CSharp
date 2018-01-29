using ChronEx.Parser;
using System;
using System.Collections.Generic;
using System.Text;
using ChronEx.Processor;

namespace ChronEx.Models.AST
{
    public abstract class StatementContainerElement:ContainerElement
    {
        public List<ElementBase> Statements { get; set; } = new List<ElementBase>();
        public override void InitializeFromParseStream(ParseProcessState state)
        {
            while (state.MoveNext() != null)
            {
                //move a token - this wll cause the next item in the steream to be created
                //if we are at EOF then return
                if(state.Current().Value.TokenType==LexedTokenType.EOF)
                {
                    return;
                }
                var newElement = (StatementElement) state.CreateCurrent();
                if(newElement != null)
                {
                    //a element was created - at this level it would be a statement
                    Statements.Add(newElement.ContainedElement);
                }
                
            }
        }

        //basic and container
        internal override MatchResult BeginProcessMatch(Tracker tracker, EventStream eventenum, List<IChronologicalEvent> CapturedList)
        {
            //container elements run thru all of the statements and apply the matches
            //when we enter here the eventenum should already be pointng at the correct event
            tracker.DebugStart(this);
            //tracker.dbgMsgs.Add(this.ToString());
            var movenextres = true;
            var myStatementsEnum = Statements.GetEnumerator();
            //move to t he first statement
            myStatementsEnum.MoveNext();

            MatchResult res = MatchResult.None;
            while (myStatementsEnum.Current!=null)
            {
                //transfer responsibility to each statement , the simple ones will just return their matches
                //the more advanced ones will take over the processing

                //if eventnum is empty 

                res = myStatementsEnum.Current.BeginProcessMatch(tracker, eventenum, CapturedList);
                //if the child did not requst to forward then move the event
                if (!res.Is_Forward())
                {
                    eventenum.MoveNext();
                }
                //if i don't match - and the element is not saying continue with me , in other words there is no hope for
                //me to match not now and not in the future

                if (!res.Is_Match() && !res.Is_Continue())
                {
                    if(tracker.DebugEnabled)
                    {
                        tracker.SaveDBGResult(this, res);
                    }
                    if(!res.Is_Forward())
                    {

                    }
                    return res;
                }
                //if the child element didn't ask to contine with its elf then move next element
                if(!res.Is_Continue())
                {
                    myStatementsEnum.MoveNext();
                }
                
                

                
                //if i already matched but there aren't any more statements in the events then just return a match
                if(res.Is_Match() && eventenum.Current==null)
                {
                    //return res;
                
                }
            }

            if (tracker.DebugEnabled)
            {
                tracker.SaveDBGResult(this, res);
            }
            //if we got here then everythign matched
            return res;
            
        }
    }
}
