using Discord;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sally_NET.Core.Enum
{
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
