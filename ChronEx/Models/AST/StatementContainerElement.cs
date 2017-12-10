using ChronEx.Parser;
using System;
using System.Collections.Generic;
using System.Text;

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
    }
}
