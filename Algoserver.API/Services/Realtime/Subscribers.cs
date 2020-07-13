using System.Collections.Generic;
using System.Threading;
using Algoserver.API.Services.Instruments;

namespace Algoserver.API.Services.Realtime
{
    public class Subscribers : HashSet<Subscription>
    {
        public Instrument Instrument { get; }
        private ReaderWriterLockSlim _syncRoot = new ReaderWriterLockSlim();

        public Subscribers(Instrument instrument = null)
        {
            Instrument = instrument;
        }

        public bool AddSubscription(Subscription subscription)
        {
            _syncRoot.EnterWriteLock();
            try
            {
                return subscription.Instrument?.Key == Instrument?.Key 
                    && !Contains(subscription) && Add(subscription);
            }
            finally
            {
                _syncRoot.ExitWriteLock();
            }
        }

        public bool RemoveSubscription(Subscription subscription)
        {
            _syncRoot.EnterWriteLock();
            try
            {
                return Remove(subscription);
            }
            finally
            {
                _syncRoot.ExitWriteLock();
            }
        }

        public void RemoveSubsriptionBySocketId(string socketId)
        {
            _syncRoot.EnterWriteLock();
            try
            {
                RemoveWhere(s => s.Client.SocketId == socketId);
            }
            finally
            {
                _syncRoot.ExitWriteLock();
            }
        }
    }
}
