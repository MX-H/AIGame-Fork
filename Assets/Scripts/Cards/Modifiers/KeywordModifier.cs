using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeywordModifier : IModifier
{
    public KeywordAttribute keywordAttribute;

    public KeywordModifier()
    {
        keywordAttribute = KeywordAttribute.NONE;
    }

    public KeywordModifier(KeywordAttribute keyword, DurationType duration) : base(duration)
    {
        keywordAttribute = keyword;
    }

    public KeywordModifier(KeywordAttribute keyword, Creature source) : base(source)
    {
        keywordAttribute = keyword;
    }
}
