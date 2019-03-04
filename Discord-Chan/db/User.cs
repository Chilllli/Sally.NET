using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;

namespace Discord_Chan.db
{
    class User
    {
        public ulong Id;
        public int Xp;
        public Timer xpTimer;
        public DateTime lastXpTime;
    }
}
