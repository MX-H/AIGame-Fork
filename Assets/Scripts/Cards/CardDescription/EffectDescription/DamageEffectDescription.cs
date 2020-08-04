using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class DamageEffectDescription : IEffectDescription
{
    public int amount;
    public DamageEffectDescription(bool heal) : base(heal ? EffectType.HEAL_DAMAGE : EffectType.DEAL_DAMAGE)
    {
    }

    public override string CardText()
    {
        return ((effectType == EffectType.HEAL_DAMAGE) ? "heal(s) " : "take(s) ") + amount.ToString() + " damage";
    }
    public override Alignment GetAlignment()
    {
        return (effectType == EffectType.HEAL_DAMAGE) ? Alignment.POSITIVE : Alignment.NEGATIVE;
    }
    public override double PowerLevel()
    {
        return ((effectType == EffectType.HEAL_DAMAGE) ? 2.0 : 1.0) * (amount - 0.5) * PowerBudget.UNIT_COST;
    }
}

public class DamageEffectProceduralGenerator : IProceduralEffectGenerator
{
    private static int MIN_DAMAGE = 1;
    private static int MAX_DAMAGE = 10;

    private readonly bool heal;
    public DamageEffectProceduralGenerator(bool heal)
    {
        this.heal = heal;
    }
    public override IEffectDescription Generate()
    {
        DamageEffectDescription desc = new DamageEffectDescription(heal);

        // Find the bounds of damage amounds
        int max = ProceduralUtils.GetUpperBound(desc, ref desc.amount, MIN_DAMAGE, MAX_DAMAGE, maxAllocatedBudget);
        int min = ProceduralUtils.GetLowerBound(desc, ref desc.amount, MIN_DAMAGE, max, minAllocatedBudget);

        Assert.IsTrue(max >= min);
        desc.amount = random.Next(min, max);

        return desc;
    }

    public override IEffectDescription GetDescriptionType()
    {
        return new DamageEffectDescription(heal);
    }

    public override double GetMinCost()
    {
        return 0.0;
    }
}
