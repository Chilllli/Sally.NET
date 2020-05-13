using Discord;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sally.NET.Core.Attr
{
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
