using ChronEx.Models;
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

        public static void AssertMatchesAreEqual(this ChronExMatches MatchList, string AssertedList)
        {
            if (MatchList == null)
            {
                throw new ArgumentNullException(nameof(MatchList));
            }

            if (string.IsNullOrWhiteSpace(AssertedList))
            {
                throw new ArgumentException("Asserted List", nameof(AssertedList));
            }

            var AssertAsList = AssertedList.Split(Environment.NewLine);
            if (MatchList.Count != AssertAsList.Count())
            {
                throw new Exception($"Expected {AssertAsList.Count()} matches but Match list contained {MatchList.Count}");
            }


            for (int i = 0; i < MatchList.Count(); i++)
            {
                MatchList[i].CapturedEvents.AssertMatchesAreEqual(AssertAsList[i]);
            }
        }
    }
}
