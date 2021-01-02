using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeywordModifierDescription : IModifierDescription
{
    public KeywordAttribute keyword;
    public KeywordModifierDescription() : base(ModifierType.KEYWORD)
    {
    }

    public override string CardText(bool plural)
    {
        return CardParsing.Parse(keyword);
    }

    public override Alignment GetAlignment()
    {
        return Alignment.POSITIVE;
    }

    public override IModifier GetModifier(DurationType duration)
    {
        return new KeywordModifier(keyword, duration);
    }

    public override IModifier GetModifier(Creature source)
    {
        return new KeywordModifier(keyword, source);
    }

    public override double PowerLevel()
    {
        return 1.5 * PowerBudget.UNIT_COST;
    }
}

public class KeywordModifierProceduralGenerator : IProceduralModifierGenerator
{
    public override IModifierDescription Generate()
    {
        KeywordModifierDescription desc = new KeywordModifierDescription();
        desc.keyword = ProceduralUtils.GetRandomValue<KeywordAttribute>(random, model);

        return desc;
    }

    public override IModifierDescription GetDescriptionType()
    {
        return new KeywordModifierDescription();
    }

    public override double GetMinCost()
    {
        return GetDescriptionType().PowerLevel();
    }
}