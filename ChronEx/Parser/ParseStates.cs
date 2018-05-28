using ChronEx.Models.AST;
using System;
using System.Collections.Generic;

namespace ChronEx.Parser
{
    /// <summary>
    /// This class is the parent class for state machines used during parsing , each type of scope has its own statemacine
    /// allow deterimation if the statement is valid and the type of AST to create
    /// </summary>
    public abstract class ParseStates
    {

        public static Tuple<StatementState, Func<ElementBase>> Ntp(StatementState tostate, Func<ElementBase> Consturct)
        {
            return new Tuple<StatementState, Func<ElementBase>>(tostate, Consturct);
        }
        /// <summary>
        /// A dictionary with delegates that create elemetns to avoid needing to use reflection
        /// </summary>
        public static Dictionary<string, Tuple<StatementState, Func<ElementBase>>> Constructors =
        new Dictionary<string,Tuple<StatementState, Func<ElementBase>>>()
        {
            {"NEGATEDSYNTAX",Ntp(StatementState.Negation,()=>new NegatedSyntax()) },
            {"NOCAPTURESYNTAX",Ntp(StatementState.NoCapture,()=>new NoCaptureSyntax()) },
            {"SYMBOLQUANTIFIER",Ntp(StatementState.SymbolQuantifier,()=>new SymbolQuantifierSyntax()) },
            {"NAMESELECTOR",Ntp(StatementState.Selector,()=>new SpecifiedEventNameSelector()) },
            {"REGEXSELECTOR",Ntp(StatementState.Selector,()=>new RegexSelector()) },
            {"STATEMENT",Ntp(StatementState.BOS,()=>new StatementElement()) },
            {"NEWLINE",Ntp(StatementState.BOS,null )},
            {"NUMERICQUANTIFIER",Ntp(StatementState.NumericQuantifierStart,()=>new NumericQuantifierSyntax()) },
            {"ANDGROUP",Ntp(StatementState.AndGroupStart,()=>new AndGroupElement()) },
            {"GROUPCLOSER",Ntp(StatementState.GroupCloser,()=>new GroupCloserPlaceHolderElement()) },
            {"NEGATEDGROUP",Ntp(StatementState.NegatedAndGroup,()=>throw new ParserException("Negated Groups are not allowed")) }
        };
        //in child parser

        public static Dictionary<StatementState, AllowedTransition> CreateTransitionTree()
        {
            
            Dictionary<StatementState, AllowedTransition> tempTree =
                new Dictionary<StatementState, AllowedTransition>();

            //BOF and newline lead to a statement
            AddToRoute(tempTree, LexedTokenType.BOF, new TransitionRecord("STATEMENT"), StatementState.BOF);
           // AddToRoute(tempTree, LexedTokenType.NEWLINE, new TransitionRecord("STATEMENT"), StatementState.EOS);
            //all the statment states that can lead to a selector
            var statesLeadingtoSelectorTypes = new StatementState[]
                {
                    StatementState.BOS,
                    StatementState.NoCapture,
                    StatementState.Negation

                };
            //add all of the selector transitions for the list above
            
            AddToRoute(tempTree, LexedTokenType.TEXT, new TransitionRecord("NAMESELECTOR"), statesLeadingtoSelectorTypes);
            AddToRoute(tempTree, LexedTokenType.DELIMITEDTEXT, new TransitionRecord("NAMESELECTOR"), statesLeadingtoSelectorTypes);
            AddToRoute(tempTree, LexedTokenType.REGEX, new TransitionRecord("REGEXSELECTOR"), statesLeadingtoSelectorTypes);
            AddToRoute(tempTree, LexedTokenType.OPENPAREN, new TransitionRecord("ANDGROUP"), statesLeadingtoSelectorTypes);
            

            //Add the states that can transition to either a EOL or a EOF
            var statesLeadingToEOFEOL = new StatementState[]
                {
                    StatementState.Selector,
                    StatementState.SymbolQuantifier,
                    StatementState.NumericQuantifierEnd,
                    StatementState.AndGroupStart,
                    StatementState.GroupCloser

                };
            //newline should transition to a new statement
            AddToRoute(tempTree, LexedTokenType.NEWLINE, new TransitionRecord("STATEMENT"), statesLeadingToEOFEOL);
            AddToRoute(tempTree, LexedTokenType.EOF, new TransitionRecord("NEWLINE"), statesLeadingToEOFEOL);

            //who can lead to negate
            AddToRoute(tempTree, LexedTokenType.EXCLAMATION, new TransitionRecord("NEGATEDSYNTAX"), StatementState.BOS,StatementState.NoCapture);
            AddToRoute(tempTree, LexedTokenType.EXCLAMATIONOPENPAREN, new TransitionRecord("NEGATEDGROUP"), StatementState.BOS, StatementState.NoCapture);
            //no capture can only really come from a BOS
            AddToRoute(tempTree, LexedTokenType.DASH, new TransitionRecord("NOCAPTURESYNTAX"), StatementState.BOS);

            //A and group closing can only be the first item on the line
            AddToRoute(tempTree, LexedTokenType.CLOSEPAREN, new TransitionRecord("GROUPCLOSER"), StatementState.BOS);

            //statements that can lead to Symbol quantifier - these are the selectors
            AddToRoute(tempTree, LexedTokenType.QUESTIONMARK, new TransitionRecord("SYMBOLQUANTIFIER"), StatementState.Selector);
            AddToRoute(tempTree, LexedTokenType.PLUS, new TransitionRecord("SYMBOLQUANTIFIER"), StatementState.Selector);
            AddToRoute(tempTree, LexedTokenType.STAR, new TransitionRecord("SYMBOLQUANTIFIER"), StatementState.Selector);
            AddToRoute(tempTree, LexedTokenType.QUESTIONMARK, new TransitionRecord("SYMBOLQUANTIFIER"), StatementState.GroupCloser);
            AddToRoute(tempTree, LexedTokenType.PLUS, new TransitionRecord("SYMBOLQUANTIFIER"), StatementState.GroupCloser);
            AddToRoute(tempTree, LexedTokenType.STAR, new TransitionRecord("SYMBOLQUANTIFIER"), StatementState.GroupCloser);

            //numeric quantifier section

            AddToRoute(tempTree, LexedTokenType.OPENCURLY, new TransitionRecord("NUMERICQUANTIFIER"), StatementState.Selector);
            AddToRoute(tempTree, LexedTokenType.OPENCURLY, new TransitionRecord("NUMERICQUANTIFIER"), StatementState.GroupCloser);
            return tempTree;
        }

        public static void AddToRoute(Dictionary<StatementState, AllowedTransition> tempTree,
            LexedTokenType tokenType,TransitionRecord transitionRecord,params StatementState[] ApplyToStates)
        {
            //create a shared transitionrecord

            foreach (var state in ApplyToStates)
            {
                //check if the state is in the dictionary already
                //if its not then add it
                if(!tempTree.ContainsKey(state))
                {
                    tempTree[state] = new AllowedTransition();
                }

                tempTree[state].Add(tokenType, transitionRecord);
            }
        }
        public static void AddSelectorsToAllowedTransition(Dictionary<StatementState, AllowedTransition> tempTree,StatementState fromstate)
        {
            var subtree = tempTree[fromstate];
            subtree.Add(LexedTokenType.REGEX, new TransitionRecord("REGEXSELECTOR"));
            subtree.Add(LexedTokenType.TEXT, new TransitionRecord("NAMESELECTOR"));
            subtree.Add(LexedTokenType.DELIMITEDTEXT, new TransitionRecord("NAMESELECTOR"));


        }

        public static Dictionary<StatementState, AllowedTransition> StateTransitions =
            CreateTransitionTree();




    }
    /// <summary>
    /// This enum identifies the state of the statement being parsed
    /// </summary>
    public enum StatementState
    {
        BOF,
        /// <summary>
        /// Begin of Statement
        /// </summary>
        BOS,
        NoCapture,
        Negation,
        Selector,
        SymbolQuantifier,
        EOS,
        // - for numeric quantifiers
        NumericQuantifierStart,
        NumericQuantifierMin,
        NumericQuantifierComma,
        NumericQuantifierMax,
        NumericQuantifierEnd,

        EOF,
        STATEMENT,
        AndGroupStart,
        GroupCloser,
        NegatedAndGroup
    }

    /// <summary>
    /// An allowed transition specifyies the allowed transition from one parser state to another
    /// it specifies which tokens are allowed to cause the transtions and what the new state will be
    /// including wether a new factory on the stack to handle the transition
    /// </summary>
    public class AllowedTransition : Dictionary<LexedTokenType, TransitionRecord>
    { }



    public class TransitionRecord
    {


        public TransitionRecord(string factory)
        {

            Factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }


        public string Factory { get; set; }

        public static TransitionRecord Blank() => new TransitionRecord(""); 
    }
}
