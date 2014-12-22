using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Company.VSWorkingSetPkg
{
    using System.Threading;

    public sealed class MonitorQueue
    {
        private object lockObject;
        private volatile int ticketsDistributed = 0;
        private volatile int ticketNowServing = 1;

        public MonitorQueue()
        {
            lockObject = new Object();
        }

        public void Enter()
        {
            int myTicketNumber = Interlocked.Increment(ref ticketsDistributed);
            Monitor.Enter(lockObject);
            while (true)
            {
                if (myTicketNumber == ticketNowServing)
                {
                    return;
                }
                else
                {
                    Monitor.Wait(lockObject);
                }
            }
        }

        public void Exit()
        {
            Interlocked.Increment(ref ticketNowServing);
            Monitor.PulseAll(lockObject);
            Monitor.Exit(lockObject);
        }
    }
}
