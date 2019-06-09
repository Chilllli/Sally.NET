using Discord;
using Discord.WebSocket;
using Sally_NET.Database;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Web;

namespace Sally_NET.Service
{
    static class MoodHandleService
    {
        private static DiscordSocketClient client;
        private static double dailyPoints;
        private static float pointsSum;
        private static List<DateTime> messageList = new List<DateTime>();
        private static bool onStart;
        public static double MoodPoints
        {
            get
            {
                return (dailyPoints + pointsSum + (Program.MyGuild.Users.ToList().Count(u => u.Status != Discord.UserStatus.Offline) / (double)Program.MyGuild.Users.Count) + (Program.MyGuild.Users.ToList().Find(u => u.VoiceChannel != null) != null ? 1 : 0) + (1 - 1 / (1 + messageCounter()))) / (onStart ? 3.0f : 5.0f);
            }
        }
        public static async Task InitializeHandler(DiscordSocketClient client)
        {
            MoodHandleService.client = client;
            //set start to true
            onStart = true;
            //Initialize Timer
            Timer dailyTimer = new Timer(24 * 60 * 60 * 1000);
            Timer weatherTimer = new Timer(8 * 60 * 60 * 1000);
            Timer changeMoodTimer = new Timer(60 * 1000);
            //hook timer events
            dailyTimer.Elapsed += DailyTimer_Elapsed;
            weatherTimer.Elapsed += WeatherTimer_Elapsed;
            changeMoodTimer.Elapsed += ChangeMoodTimer_Elapsed;
            //start timmer
            dailyTimer.Start();
            weatherTimer.Start();
            //get start values
            DailyTimer_Elapsed(null, null);
            await checkWeather();
            await setMood(getMood());
            onStart = false;
            client.MessageReceived += Client_MessageReceived;
        }

        private static async void ChangeMoodTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            await setMood(getMood());
        }

        private static Task Client_MessageReceived(SocketMessage message)
        {
            messageList.Add(message.CreatedAt.DateTime);
            messageList = messageList.Where(t => DateTime.Now.Subtract(new TimeSpan(0, 5, 0)) > t).ToList();
            return Task.CompletedTask;
        }

        private static async void WeatherTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            await checkWeather();
        }

        private static void DailyTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            dailyPoints = new Random().NextDouble();
        }

        private static async Task checkWeather()
        {
            dynamic temperature = JsonConvert.DeserializeObject<dynamic>(ApiRequestService.StartRequest("weatherapi"));
            //main.temp 60%,
            pointsSum = calculateWeatherPoints(15f, 20f, 0.6f, (float)temperature.main.temp);
            //main.humidity 5%, 
            pointsSum += calculateWeatherPoints(10f, 50f, 0.05f, (float)temperature.main.humidity);
            //wind.speed 5%,
            pointsSum += calculateWeatherPoints(10f, 4f, 0.05f, (float)temperature.wind.speed);
            //clouds.all 10%,
            pointsSum += calculateWeatherPoints(50f, 10f, 0.1f, (float)temperature.clouds.all);
            //rain.1h 0.2 10%,
            pointsSum += calculateWeatherPoints(2.5f, 0f, 0.1f, temperature.rain != null ? (float)temperature.rain["1h"] : 0f);
            //snow.1h 0.1 10% 
            pointsSum += calculateWeatherPoints(2.5f, 0f, 0.1f, temperature.snow != null ? (float)temperature.snow["1h"] : 0f);
        }
        private static float calculateWeatherPoints(float width, float optimum, float weigth, float weatherValue)
            => MathF.Max(-(1 / MathF.Pow(width, 2)) * MathF.Pow((weatherValue - optimum), 2) + 1, -1) * weigth;

        private static int messageCounter()
        {
            return messageList.Count(m => m > DateTime.Now.Subtract(new TimeSpan(0, 1, 0)));
        }

        public static Mood getMood()
        {
            double currentMood = MoodPoints;
            if (currentMood >= 0 && currentMood <= 0.25)
            {
                return Mood.Sad;
            }
            if (currentMood > 0.25 && currentMood <= 0.50)
            {
                return Mood.Meh;
            }
            if (currentMood > 0.50 && currentMood <= 0.75)
            {
                return Mood.Happy;
            }
            return Mood.Extatic;
        }

        public enum Mood
        {
            Sad,
            Meh,
            Happy,
            Extatic
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        private static async Task setMood(Mood mood)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            if (client.Activity?.Name == mood.ToString())
            {
                return;
            }
            DataAccess.Instance.saveMood(mood);
#if RELEASE
            await client.SetActivityAsync(new Game(mood.ToString()));
            await client.CurrentUser.ModifyAsync(c => c.Avatar = new Image($"./mood/{mood}.png"));
#endif
        }
    }
}
