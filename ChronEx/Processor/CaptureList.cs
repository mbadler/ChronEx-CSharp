using ChronEx.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChronEx.Processor
{
    public class CaptureList
    {
        protected List<IChronologicalEvent> _List = new List<IChronologicalEvent>();
        
        public void Add(IChronologicalEvent evt)
        {
            _List.Add(evt);
        }

        public void AddRange(IEnumerable<IChronologicalEvent> range)
        {
            _List.AddRange(range);
        }

        public IEnumerable<IChronologicalEvent> Items
        {
            get
            {
                return _List;
            }
        }

        internal  List<IChronologicalEvent> InternalList()
        {
            return _List;
        }
    }
}
