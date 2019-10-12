using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Sally_NET.Service;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using Newtonsoft.Json.Linq;
using System.Net;

namespace Sally_NET.Command
{
    public class GameCommands : ModuleBase
    {
        [Group("terraria")]
        public class TerrariaServer : ModuleBase
        {
            [Command("mods")]
            public async Task GetEnabledMods()
            {
                SocketGuildUser guildUser = Context.Message.Author as SocketGuildUser;
                if (guildUser == null || guildUser.Roles.ToList().Find(r => r.Id == Program.BotConfiguration.TerrariaId) == null)
                {
                    return;
                }
#if DEBUG
                string[] mods = JsonConvert.DeserializeObject<string[]>(File.ReadAllText("debug.json"));
#else
                string[] mods = JsonConvert.DeserializeObject<string[]>(File.ReadAllText("/srv/terraria/.local/share/Terraria/ModLoader/Mods/enabled.json"));
#endif
                await Context.Message.Channel.SendMessageAsync(null, embed: modInfoEmbed(mods)).ConfigureAwait(false);
            }
        }
        private static Embed modInfoEmbed(string[] mods)
        {
            EmbedBuilder dynamicEmbed = new EmbedBuilder()
                .WithTitle("Terraria Server")
                .WithDescription("Mods, which are currently running on the server")
                .WithColor(Color.Green)
                .WithTimestamp(DateTime.Now)
                .WithFooter(footer =>
                {
                    footer
                    .WithText($"There are currently {mods.Length} mods.");
                });
            foreach (string mod in mods)
            {
                dynamicEmbed.AddField(mod, "\u2705", true);
            }
            return dynamicEmbed.Build();
        }
        [Group("rl")]
        public class RocketLeagueCommands : ModuleBase
        {
            [Command("setRank")]
            public async Task setPlayerRank(Rank rank, int level = 0)
            {
                if (level != 0 && rank == Rank.GrandChampion)
                {
                    await Context.Message.Channel.SendMessageAsync("invalid operation");
                    return;
                }
                if (level > 3 || level < 1)
                {
                    await Context.Message.Channel.SendMessageAsync("invalid rank level");
                    return;
                }
                Type enumType = typeof(Rank);
                MemberInfo[] memInfo = enumType.GetMember(rank.ToString());
                Object[] attributes = memInfo[0].GetCustomAttributes(typeof(RankAttribute), false);
                Color color = ((RankAttribute)attributes[0]).color;

                RoleManagerService.CreateOrAddRole(rank != Rank.GrandChampion ? $"{rank} {level}" : rank.ToString(), Context.Message.Author.Id, Enum.GetNames(typeof(Rank)), color);
            }
            public enum Rank
            {
                [RankAttribute(0x77391a)] Wood,
                [RankAttribute(0xc95d1a)] Bronze,
                [RankAttribute(0x5eedea)] Silver,
                [RankAttribute(0xd2d60a)] Gold,
                [RankAttribute(0x62c4ba)] Platinum,
                [RankAttribute(0x1653e2)] Diamond,
                [RankAttribute(0x801aed)] Champion,
                [RankAttribute(0x801aed)] GrandChampion
            }
            public class RankAttribute : Attribute
            {
                public uint HexColor;
                public Color color
                {
                    get
                    {
                        return new Color(HexColor);
                    }
                }
                public RankAttribute(uint hexColor)
                {
                    HexColor = hexColor;
                    Color color = new Color(hexColor);
                }
            }
        }

        [Group("rs")]
        public class RuneScapeCommands : ModuleBase
        {
            [Command("value")]
            public async Task CheckPrice(string name)
            {
                ////create a generic text format
                //TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;

                ////normalize text input
                //string normInput = textInfo.ToTitleCase(name);

                string lowerName = name.ToLower();
                string normInput = lowerName.First().ToString().ToUpper() + lowerName.Substring(1);
                string itemUrl = normInput.Replace(" ", "_");

                IMessage searchMessage = Context.Message.Channel.SendMessageAsync("Searching for item....").Result;

                using (WebClient wc = new WebClient())
                {
                    string id = null;
                    var json = wc.DownloadString("https://rsbuddy.com/exchange/summary.json");
                    JObject jsonIdFinder = JObject.Parse(json);
                    JObject jsonItem = null;
                    string json2 = null;
                    Dictionary<string, int> itemNameComparison = new Dictionary<string, int>();
                    bool hasBreaked = false;
                    //make foreach smaller. need to overthink the code body of foreach
                    foreach (var item in jsonIdFinder)
                    {
                        string dataItemName = (string)jsonIdFinder[item.Key]["name"];
                        if (!itemNameComparison.ContainsKey(dataItemName))
                        {
                            itemNameComparison.Add(dataItemName, CommandHandlerService.CalcLevenshteinDistance(dataItemName, normInput));
                        }

                        if (normInput == dataItemName && !hasBreaked)
                        {
                            EmbedBuilder rsEmbed = new EmbedBuilder();
                            var parentKey = item.Value.AncestorsAndSelf()
                                                .FirstOrDefault(k => k != null);
                            id = (string)parentKey["id"];
                            try
                            {
                                json2 = wc.DownloadString($"https://services.runescape.com/m=itemdb_rs/api/catalogue/detail.json?item={id}");
                            }
                            catch (Exception)
                            {
                                json2 = null;
                            }


                            if (json2 == null)
                            {
                                jsonItem = new JObject();
                                rsEmbed
                                .WithTitle("Oldschool Runescape Grand Exchange Price Check")
                                .WithDescription("Check current prices of items in the grand exchange")
                                .WithColor(Color.DarkGreen)
                                .WithTimestamp(DateTime.Now)
                                .WithThumbnailUrl($"https://oldschool.runescape.wiki/images/thumb/7/72/{itemUrl}_detail.png/130px-Dragon_longsword_detail.png?7052f")
                                .WithFooter(Program.GenericFooter, Program.GenericThumbnailUrl)
                                .AddField("Name", (string)jsonIdFinder[item.Key]["name"], true)
                                .AddField("Member-Item", (string)jsonIdFinder[item.Key]["members"] == "true" ? "\u2705" : "\u274E", true)
                                .AddField("Buying Price", (string)jsonIdFinder[item.Key]["buy_average"] + " gp", true)
                                .AddField("Selling Price", (string)jsonIdFinder[item.Key]["sell_average"] + " gp", true)
                                .AddField("Further Reading", $"https://oldschool.runescape.wiki/w/{itemUrl}");
                            }
                            else
                            {
                                jsonItem = JObject.Parse(json2);
                                rsEmbed
                                .WithTitle("Oldschool Runescape Grand Exchange Price Check")
                                .WithDescription("Check current prices of items in the grand exchange")
                                .WithColor(Color.DarkGreen)
                                .WithTimestamp(DateTime.Now)
                                .WithThumbnailUrl($"https://oldschool.runescape.wiki/images/thumb/7/72/{itemUrl}_detail.png/130px-Dragon_longsword_detail.png?7052f")
                                .WithFooter(Program.GenericFooter, Program.GenericThumbnailUrl)
                                .AddField("Name", (string)jsonItem["item"]["name"], true)
                                .AddField("Type", (string)jsonItem["item"]["type"], true)
                                .AddField("Description", (string)jsonItem["item"]["description"], true)
                                .AddField("Member-Item", (string)jsonItem["item"]["members"] == "true" ? "\u2705" : "\u274E", true)
                                .AddField("Current Value", (string)jsonItem["item"]["current"]["price"] + " gp")
                                .AddField("Buying Price", (string)jsonIdFinder[item.Key]["buy_average"] + " gp", true)
                                .AddField("Selling Price", (string)jsonIdFinder[item.Key]["sell_average"] + " gp", true)
                                .AddField("30 Days Price Trend", (string)jsonItem["item"]["day30"]["change"])
                                .AddField("90 Days Price Trend", (string)jsonItem["item"]["day90"]["change"])
                                .AddField("180 Days Price Trend", (string)jsonItem["item"]["day180"]["change"])
                                .AddField("Further Reading", $"https://oldschool.runescape.wiki/w/{itemUrl}");
                            }


                            await searchMessage.DeleteAsync();
                            await Context.Message.Channel.SendMessageAsync(embed: rsEmbed.Build());
                            hasBreaked = true;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    if (jsonItem == null)
                    {
                        int minValue = itemNameComparison.Values.Min();
                        string result = itemNameComparison.Where(v => v.Value == minValue).FirstOrDefault().Key;
                        await searchMessage.DeleteAsync();
                        await Context.Message.Channel.SendMessageAsync($"Item not found... But do you mean {result}?");
                    }
                }
            }
        }
    }
}
