using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace ChronEx.Models
{
    [DebuggerDisplay("EventName = {EventName} , DateTime = {EventDateTime}")]
    public class ChronologicalEvent : IChronologicalEvent
    {
        public string EventName { get; set; }
        public DateTime? EventDateTime { get; set; }

        public string Describe()
        {
            return $"EventName = {EventName} , DateTime = {EventDateTime}";
        }
    }
}
