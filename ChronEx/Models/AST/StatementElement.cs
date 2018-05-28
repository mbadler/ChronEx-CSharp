using System;
using System.Collections.Generic;
using System.Text;
using ChronEx.Parser;
using ChronEx.Processor;
using System.Linq;

namespace ChronEx.Models.AST
{
    /// <summary>
    /// High level container per statement levle , for parsing purposes only
    /// forwards all calls downwards - this element get created during parsin g, but its first child element is what gets added tot the ast tree
    /// Supports subelement self ordering
    /// </summary>
    public class StatementElement : ContainerElement
    {
        public override int ZOrder => 0;

        public override string Describe()
        {
            return "Statement";
        }

        private List<ElementBase> unorderdelements = new List<ElementBase>();
        public override void InitializeFromParseStream(ParseProcessState state)
        {
         
            while (state.MoveNext(true) != null)
            {
                //move a token - this wll cause the next item in the steream to be created
                // var nextToken = state.MoveNext();
                var newElement = state.CreateCurrent();
                
                if (newElement != null)
                {
                    //a element was created - at this level it would be a statement
                    unorderdelements.Add(newElement);
                }
                //if iis a close group token (such as ) or ] ) then exit here to slide back to the parent
                if (newElement is GroupCloserPlaceHolderElement)
                {
                    ContainedElement = newElement;
                    return;
                }
                //peek next ifits a eol or a eof then orginize the statement and return
                var nxt = state.Peek(1);
                if(nxt==null)
                {
                    throw new ParseException("Statment ended with a null, EOF or NEWLINE expected");
                }
                var nxtval = nxt.Value.TokenType;
                if(nxtval == LexedTokenType.NEWLINE||nxtval == LexedTokenType.EOF)
                {
                    ArrangeContainedElements();
                    return;
                }
            }
        }

        private void ArrangeContainedElements()
        {
            ContainerElement parentEleme = this;
            //order the unorderdelements by zorder
            //and start building the tree by pluggin each element under it
            foreach(var elem in unorderdelements.OrderByDescending(x=>x.ZOrder))
            {
                parentEleme.AddContainedElement(elem);
                if(elem is ContainerElement)
                {
                    parentEleme = (ContainerElement)elem;
                }
            }
        }

        internal override MatchResult IsMatch(IChronologicalEvent chronevent, Tracker Tracker, CaptureList CapturedList)
        {
            return ContainedElement.IsMatch(chronevent, Tracker, CapturedList);
        }

        internal override bool IsPotentialMatch(IChronologicalEvent chronevent)
        {
            return IsPotentialMatch(chronevent);
        }
    }
}
