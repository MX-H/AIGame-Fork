using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class SummonEffectDescription : IEffectDescription
{
    public int amount;
    public CreatureType tokenType;
    public SummonEffectDescription() : base(EffectType.SUMMON_TOKEN)
    { }
    public override string CardText()
    {
        return "summon(s) " + amount.ToString() + " " + CardParsing.Parse(tokenType) + " token(s)";
    }

    public override Alignment GetAlignment()
    {
        return Alignment.POSITIVE;
    }

    public override double PowerLevel()
    {
        return (amount - 0.5) * PowerBudget.UNIT_COST;
    }
}

public class SummonEffectProceduralGenerator : IProceduralEffectGenerator
{
    private static int MIN_SUMMONS = 1;
    private static int MAX_SUMMONS = 5;
    
    public override IEffectDescription Generate()
    {
        SummonEffectDescription desc = new SummonEffectDescription();
        desc.tokenType = ProceduralUtils.GetRandomValue<CreatureType>(random, model);

        // Find the bounds of card amounts
        int max = ProceduralUtils.GetUpperBound(desc, ref desc.amount, MIN_SUMMONS, MAX_SUMMONS, maxAllocatedBudget);
        int min = ProceduralUtils.GetLowerBound(desc, ref desc.amount, MIN_SUMMONS, max, minAllocatedBudget);

        Assert.IsTrue(max >= min);
        desc.amount = random.Next(min, max);

        return desc;
    }

    public override IEffectDescription GetDescriptionType()
    {
        return new SummonEffectDescription();
    }

    public override double GetMinCost()
    {
        return PowerBudget.UNIT_COST;
    }
}