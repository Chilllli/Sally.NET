using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sally.NET.Service.Subscription
{
    interface ISubscription
    {
        void Subscribe(SocketSelfUser user, object subscribeObject);
        void Notify(SocketSelfUser user, object notifyObject);
    }
}
