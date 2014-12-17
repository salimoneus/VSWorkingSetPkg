using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Company.VSWorkingSetPkg
{
    using System.Threading;

    public sealed class QueuedLock
    {
        private object innerLock;
        private volatile int ticketsCount = 0;
        private volatile int ticketToRide = 1;

        public QueuedLock()
        {
            innerLock = new Object();
        }

        public void Enter()
        {
            #pragma warning disable 420
            int myTicket = Interlocked.Increment(ref ticketsCount);
            #pragma warning restore 420
            Monitor.Enter(innerLock);
            while (true)
            {

                if (myTicket == ticketToRide)
                {
                    return;
                }
                else
                {
                    Monitor.Wait(innerLock);
                }
            }
        }

        public void Exit()
        {
            #pragma warning disable 420
            Interlocked.Increment(ref ticketToRide);
            #pragma warning restore 420
            Monitor.PulseAll(innerLock);
            Monitor.Exit(innerLock);
        }
    }
}
