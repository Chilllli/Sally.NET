using Discord;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sally.NET.Core;
using Sally.NET.DataAccess.Database;
using Sally.NET.Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Sally.NET.Module
{
    public class GameModule
    {
        private readonly Helper helper;
        private readonly IDBAccess dBAccess;
        public GameModule(Helper helper, IDBAccess dBAccess)
        {
            this.helper = helper;
            this.dBAccess = dBAccess;
        }
        /// <summary>
        /// Returns a string array of enabled terraria mods
        /// 
        /// </summary>
        /// <param name="file">File, where enabled terraria mods are stored e.g.: enabled.json</param>
        /// <returns>string[]</returns>
        public string[] GetTerrariaMods(string file)
        {
            return JsonConvert.DeserializeObject<string[]>(File.ReadAllText(file));
        }

        public Embed TryGetRsItemPrice(string name, ulong userId, out string suggestion)
        {
            ////create a generic text format
            //TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;

            ////normalize text input
            //string normInput = textInfo.ToTitleCase(name);
            suggestion = String.Empty;
            string lowerName = name.ToLower();
            string normInput = lowerName.First().ToString().ToUpper() + lowerName[1..];
            string itemUrl = normInput.Replace(" ", "_");

            //use api service instead
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
                    //fill dictionary with item names
                    string dataItemName = (string)jsonIdFinder[item.Key]["name"];
                    if (!itemNameComparison.ContainsKey(dataItemName))
                    {
                        itemNameComparison.Add(dataItemName, helper.CalcLevenshteinDistance(dataItemName, normInput));
                    }
                    //check if the item is equal to the input
                    if (normInput == dataItemName && !hasBreaked)
                    {
                        EmbedBuilder rsEmbed = new EmbedBuilder();
                        var parentKey = item.Value.AncestorsAndSelf()
                                            .FirstOrDefault(k => k != null);
                        id = (string)parentKey["id"];
                        //for some reason the official api has not every item. check the response
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
                            //TODO: fix user color
                            rsEmbed
                            .WithTitle("Oldschool Runescape Grand Exchange Price Check")
                            .WithDescription("Check current prices of items in the grand exchange")
                            .WithColor(new Color((uint)Convert.ToInt32("ff6600", 16)))
                            .WithTimestamp(DateTime.Now)
                            .WithThumbnailUrl($"https://oldschool.runescape.wiki/images/thumb/7/72/{itemUrl}_detail.png/130px-Dragon_longsword_detail.png?7052f")
                            .WithFooter(NET.DataAccess.File.FileAccess.GENERIC_FOOTER, NET.DataAccess.File.FileAccess.GENERIC_THUMBNAIL_URL)
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
                            .WithColor(new Color((uint)Convert.ToInt32("ff6600", 16)))
                            .WithTimestamp(DateTime.Now)
                            .WithThumbnailUrl($"https://oldschool.runescape.wiki/images/thumb/7/72/{itemUrl}_detail.png/130px-Dragon_longsword_detail.png?7052f")
                            .WithFooter(NET.DataAccess.File.FileAccess.GENERIC_FOOTER, NET.DataAccess.File.FileAccess.GENERIC_THUMBNAIL_URL)
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
                        return rsEmbed.Build();
                    }
                    else
                    {
                        continue;
                    }
                }
                if (jsonItem == null)
                {
                    //if item wasnt found, give the user an item, which may be correct
                    suggestion = itemNameComparison.Where(v => v.Value == itemNameComparison.Values.Min()).FirstOrDefault().Key;
                }
                return null;
            }
        }
    }
}
