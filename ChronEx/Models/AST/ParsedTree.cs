using System;
using System.Collections.Generic;
using System.Text;

namespace ChronEx.Models.AST
{
    // the root tree returned after a parsing session
    // will contain all of the parsed elements
    public class ParsedTree
    {
        List<ElementBase> _elementList = new List<ElementBase>();
        public void AddElement(ElementBase newElement)
        {
            _elementList.Add(newElement);
        }

        public IEnumerable<ElementBase> GetElements()
        {
            var a = _elementList.GetEnumerator();
            
            while(a.MoveNext())
            {
                yield return a.Current;
            }
        }
    }
}
