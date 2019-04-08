using Discord;
using Discord.WebSocket;
using Discord_Chan.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;

namespace Discord_Chan.Service
{
    class StatusNotifierService
    {
        public static void InitializeService()
        {
#pragma warning disable CS4014 // Da dieser Aufruf nicht abgewartet wird, wird die Ausführung der aktuellen Methode fortgesetzt, bevor der Aufruf abgeschlossen ist
            Task.Run(observeNotifierPipe);
#pragma warning restore CS4014 // Da dieser Aufruf nicht abgewartet wird, wird die Ausführung der aktuellen Methode fortgesetzt, bevor der Aufruf abgeschlossen ist
        }

        private static async Task observeNotifierPipe()
        {
            while (true)
            {
                using (NamedPipeServerStream npss = new NamedPipeServerStream("StatusNotifier", PipeDirection.In))
                {
                    npss.WaitForConnection();
                    using (StreamReader reader = new StreamReader(npss))
                    {
                        await Program.Me.SendMessageAsync(reader.ReadLine());
                    }
                }
            }
        }
    }
}
