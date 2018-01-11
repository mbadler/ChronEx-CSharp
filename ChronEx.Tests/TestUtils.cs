using ChronEx.Models;
using ChronEx.Parser;
using ChronEx.Processor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChronEx.Tests
{
    public static class TestUtils
    {
        public static IEnumerable<ChronologicalEvent> SplitLogsStringsIntoChronEventList(string s)
        {
            return s.Split('\n').Select(x => x.Split(','))
                            .Select(y => new ChronologicalEvent()
                            {
                                EventName = y[0],
                                EventDateTime = DateTime.Parse(y[1])
                            });
        }

        public static IEnumerable<ChronologicalEvent> ChronListFromString(string s)
        {
            var a = DateTime.MinValue;

            return s.Split(' ')
                            .Select((y) =>
                            {a = a.AddSeconds(1);
                                return new ChronologicalEvent()
                                {
                                    EventName = y,
                                    EventDateTime = a
                                };
                                
                            });
        }

        public static void AssertMatchesAreEqual(this List<IChronologicalEvent> MatchList,string AssertedList)
        {
            if (MatchList == null)
            {
                throw new ArgumentNullException(nameof(MatchList));
            }

            if (string.IsNullOrWhiteSpace(AssertedList))
            {
                throw new ArgumentException("Asserted List", nameof(AssertedList));
            }

            var AssertAsList = AssertedList.Split(",");
            if(MatchList.Count != AssertAsList.Count())
            {
                var expect = String.Join(",", AssertAsList);
                var resul = String.Join(",", MatchList.Select(x => x.EventName));
                throw new Exception($"Expected {expect} but the result was {resul} ");
            }

            for (int i = 0; i < MatchList.Count(); i++)
            {
                if(AssertAsList[i] != MatchList[i].EventName)
                {
                    var expect = String.Join(",", AssertAsList);
                    var resul = String.Join(",", MatchList.Select(x=>x.EventName));
                    throw new Exception($"For index {i} : Expected {expect} but the result was {resul} ");
                }
            }
        }

        public static void AssertNoMatches(this ChronExMatches MatchList)
        {
            if(MatchList.Any())
            {
                throw new Exception("No matches were expected but the following were found:" +
                DescribeMatchList(MatchList));

            }
        }

        private static string DescribeMatchList(ChronExMatches MatchList)
        {
            
               return string.Join('\n', MatchList.Select(x => string.Join(",", x.CapturedEvents.Select(y => y.EventName))));
        }

        public static void AssertMatchesAreEqual(this ChronExMatches MatchList, string AssertedList)
        {
            if (MatchList == null)
            {
                throw new ArgumentNullException(nameof(MatchList));
            }

            //if (string.IsNullOrWhiteSpace(AssertedList))
            //{
            //    throw new ArgumentException("Asserted List", nameof(AssertedList));
            //}

            var AssertAsList = AssertedList.Split(Environment.NewLine);
            if (MatchList.Count != AssertAsList.Count())
            {
                throw new Exception($"Expected {AssertAsList.Count()} matches but Match list contained {MatchList.Count} \n{DescribeMatchList(MatchList)}");
            }


            for (int i = 0; i < MatchList.Count(); i++)
            {
                MatchList[i].CapturedEvents.AssertMatchesAreEqual(AssertAsList[i]);
            }
        }

        public static void AssertTokenTypeIs(this LexedToken token,LexedTokenType ExpectedTokenType)
        {
            if (token.TokenType != ExpectedTokenType)
            {
                throw new Exception(String.Format("Token type {0} Expected but {1} found", token.TokenType.ToString(), ExpectedTokenType.ToString()));
            }
        }

        public static void AssertTokenIs(this LexedToken token, LexedTokenType ExpectedTokenType,object ExpectedTokenValue)
        {
            token.AssertTokenTypeIs(ExpectedTokenType);
            if (token.TokenText != ExpectedTokenValue.ToString() )
            {
                throw new Exception(String.Format("Token value {0} Expected but {1} found", ExpectedTokenValue.ToString(), token.TokenText.ToString()));
            }

        }
    }
}
