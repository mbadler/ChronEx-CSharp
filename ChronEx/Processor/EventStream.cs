using ChronEx.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace ChronEx.Processor
{
    public interface IEventStream : IEnumerator<IChronologicalEvent>
    {
        SpeculativeEventStream CreateSpeculator();
        void EndApplyAll();
        void EndDiscardAll();
        void CofirmUntilIndex(int index);

        
    }
    public class EventStream : IEventStream
    {
        protected List<IChronologicalEvent> SpeculationBuffer = new List<IChronologicalEvent>();
        IEnumerator<IChronologicalEvent> _events = null;
        int BufferStartAt = 0; //the index where the first entry in the buffer starts
        int SpeculatorIndex = 0;
        int ConcreteEventIndex = 0; //the index of the current concrete event
        
        
        
        bool HasEventsInBuffer = false;



        bool HasActiveSpeculator = false;

        public EventStream(IEnumerable<IChronologicalEvent> Events)
        {
            if (Events != null)
            {
                _events = Events.GetEnumerator();
            }

        }

        public virtual IChronologicalEvent Current
        {

            get
            {
                if (HasActiveSpeculator)
                {
                    return SpeculativeCurrent();
                }

                if(HasEventsInBuffer)
                {
                    
                    return SpeculationBuffer[(ConcreteEventIndex-1) - BufferStartAt ];
                }
                else
                {
                    
                    return _events.Current;
                }
                //simpelist case - we are normal not speculator and no buffer
                
            }




        }

        private IChronologicalEvent SpeculativeCurrent()
        {
            int maxeventIndex = MaxEventIndex();
            return SpeculationBuffer[(SpeculatorIndex -1)- BufferStartAt];//  ((maxeventIndex - SpeculatorIndex) + (BufferStartAt - 1))];
        }

        private int MaxEventIndex()
        {
            return SpeculationBuffer.Count + BufferStartAt;
        }

        object IEnumerator.Current => throw new NotImplementedException();

        public void Dispose()
        {
            if (_events != null)
            {
                _events.Dispose();
            }
        }

        public int ConcreatedIndex
        {
            get
            {
                return ConcreteEventIndex;
            }
        }

        public virtual bool MoveNext()
        {
            //If we still have events in the buffer then deliver those 

            //simplest scenerio , no syndicator
            if (HasActiveSpeculator)
            {
                return SpeculativeMoveNext();
            }
            if (!HasEventsInBuffer)
            {
                var a = _events.MoveNext();
                if(a)
                {
                    ConcreteEventIndex++;
                    
                }
                return a;
            }
            else
            {
                
                
                ConcreteEventIndex++;
                if (ConcreteEventIndex-1 == BufferStartAt+SpeculationBuffer.Count)
                {
                    //we completed streaming the buffer 
                    HasEventsInBuffer = false;
                    SpeculationBuffer.Clear();
                    return _events.MoveNext();

                }
                return true;

            }
             
            
        }

        public void Reset()
        {
            _events.Reset();
        }

        public SpeculativeEventStream CreateSpeculator()
        {
            //SpeculationBuffer.Clear();
            HasActiveSpeculator = true;
            SpeculatorIndex = ConcreteEventIndex;
            if (SpeculationBuffer.Count == 0)
            {
                BufferStartAt = ConcreteEventIndex;
            }
            

            return new SpeculativeEventStream(this,ConcreteEventIndex);
        }

        

        

        public bool SpeculativeMoveNext()
        {
            
           if(SpeculatorIndex == MaxEventIndex())
            {
                var a = this._events.MoveNext();
                if(!a)
                {
                    return false;
                }
                var v = this._events.Current;
                SpeculationBuffer.Add(v);
                SpeculatorIndex++;
                HasEventsInBuffer = true;
            }
           else
            {
                //its just consuming events that we ate before
                SpeculatorIndex++;
                
            }
            return true;
        }

        public void EndApplyAll()
        {
            //clear everything back to normal
            HasActiveSpeculator = false;
            SpeculationBuffer.Clear();
            HasEventsInBuffer = false;
        }

        public void EndDiscardAll()
        {
            HasActiveSpeculator = false;

        }

        public void CofirmUntilIndex(int index)
        {
            ConcreteEventIndex = index;
        }
    }

    public class SpeculativeEventStream : IEventStream
    {
        IEventStream parentStream = null;
        private int startEventNumber = 0;
        private int CurrentEventNumber = 0;
        private int SubSpeculatorIndex = 0;
        private bool hasSubSpeculator = false;
        private bool completed = false;

        private void throwifcompleted()
        {
            if (completed)
                throw new Exception("Speculator has completed , cannot call any methods on it");
        }
        internal SpeculativeEventStream(IEventStream ParentStream,int StartEventNumber) 
        {
            parentStream = ParentStream;
            startEventNumber = StartEventNumber;
            CurrentEventNumber = StartEventNumber;
        }

        

        object IEnumerator.Current => throw new NotImplementedException();

        public IChronologicalEvent Current
        {
            get
            {
                throwifcompleted();
                return parentStream.Current;
            }
        }

        public SpeculativeEventStream CreateSpeculator()
        {
            throwifcompleted();
            hasSubSpeculator = true;
            return new SpeculativeEventStream(this, this.CurrentEventNumber);
        }

        public void EndSpeculatr(bool Rewind)
        {
            throw new NotImplementedException();
        }

       

        public bool MoveNext()
        {
            throwifcompleted();
            //pass this on to the parent
            var v = parentStream.MoveNext();
            if(!v)
            {
                return false;
            }
            //if we have a subspeculator then credit it to the subspecular
            if(hasSubSpeculator)
            {
                SubSpeculatorIndex++;
            }
            else
            {
                CurrentEventNumber++;
            }
            return true;
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~SpeculativeEventStream() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        public void EndApplyAll()
        {
            throwifcompleted();
            if(hasSubSpeculator)
            {
                doSubSpeculatorEndApplyAll();
                return;
            }
            completed = true;
            parentStream.EndApplyAll();
        }

        private void doSubSpeculatorEndApplyAll()
        {
            parentStream.CofirmUntilIndex(this.SubSpeculatorIndex);
            hasSubSpeculator = false;
        }

        public void EndDiscardAll()
        {
            throwifcompleted();
            if (hasSubSpeculator)
            {
                doSubSpeculatorEndDiscardAll();
                return;
            }
            completed = true;
            parentStream.EndDiscardAll();
        }

        private void doSubSpeculatorEndDiscardAll()
        {
            throw new NotImplementedException();
        }

        public void CofirmUntilIndex(int index)
        {
            parentStream.CofirmUntilIndex(index);
        }
        #endregion





    }

}
