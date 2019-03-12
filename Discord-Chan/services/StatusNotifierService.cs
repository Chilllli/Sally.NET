using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;

namespace Discord_Chan.services
{
    class StatusNotifierService
    {
        private static SocketUser me;
        private static readonly ulong meId = 249680382499225600;

        public static async Task InitializeService()
        {
            //finding myself
            foreach (SocketUser user in Program.MyGuild.Users)
            {
                if (user.Id == meId)
                {
                    me = user;
                    break;
                }
            }
#pragma warning disable CS4014 // Da dieser Aufruf nicht abgewartet wird, wird die Ausführung der aktuellen Methode fortgesetzt, bevor der Aufruf abgeschlossen ist
            Task.Run(observeNotifierPipe());
#pragma warning restore CS4014 // Da dieser Aufruf nicht abgewartet wird, wird die Ausführung der aktuellen Methode fortgesetzt, bevor der Aufruf abgeschlossen ist
        }

        private static Action observeNotifierPipe()
        {
            while (true)
            {
                using (NamedPipeServerStream npss = new NamedPipeServerStream("StatusNotifier", PipeDirection.In))
                {
                    npss.WaitForConnection();
                    using (StreamReader reader = new StreamReader(npss))
                    {
                        me.SendMessageAsync(reader.ReadLine());
                    }
                }
            }
        }
    }
}
