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

    public override void ApplyToTarget(Targettable target, PlayerController player)
    {
        return;
    }

    public override string CardText(bool plural)
    {
        return (plural ? "summons " : "summon ") + amount.ToString() + " " + CardParsing.Parse(tokenType) + ((amount == 1) ? " token" : " tokens");
    }

    public override Alignment GetAlignment()
    {
        return Alignment.POSITIVE;
    }

    public override double PowerLevel()
    {
        return 3 * (amount - 0.5) * PowerBudget.UNIT_COST;
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
        SummonEffectDescription desc = new SummonEffectDescription();
        desc.amount = 1;
        return desc.PowerLevel();
    }
}