using System;
using System.Collections.Generic;
using System.Text;

namespace Sally_NET.Core.Enum
{
    public class RatingShortCutAttribute : Attribute
    {
        public char ShortCut;
        public RatingShortCutAttribute(char shortCut)
        {
            ShortCut = shortCut;
        }
    }

    //Enum for Image Rating Classification
    public enum Rating
    {
        None = 0,
        [RatingShortCut('s')] Safe,
        [RatingShortCut('q')] Questionable,
        [RatingShortCut('e')] Explicit
    }
}
