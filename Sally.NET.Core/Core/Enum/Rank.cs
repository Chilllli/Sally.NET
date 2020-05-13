using Discord;
using Sally.NET.Core.Attr;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sally.NET.Core.Enum
{
    public enum Rank
    {
        [Rank(0x77391a)] Wood,
        [Rank(0xc95d1a)] Bronze,
        [Rank(0x5eedea)] Silver,
        [Rank(0xd2d60a)] Gold,
        [Rank(0x62c4ba)] Platinum,
        [Rank(0x1653e2)] Diamond,
        [Rank(0x801aed)] Champion,
        [Rank(0x801aed)] GrandChampion
    }
    
}
