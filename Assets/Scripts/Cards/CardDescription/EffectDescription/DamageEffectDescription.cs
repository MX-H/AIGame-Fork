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

    public override void ApplyToTarget(Targettable target, PlayerController player)
    {
        GameSession gameSession = GameUtils.GetGameSession();
        if (effectType == EffectType.HEAL_DAMAGE)
        {
            gameSession.ServerHealDamage(target, amount);
        }
        else
        {
            gameSession.ServerApplyDamage(target, amount);
        }
    }

    public override string CardText(bool plural)
    {
        if (effectType == EffectType.HEAL_DAMAGE)
        {
            return "heal " + amount + " health to";
        }
        else
        {
            return "deal " + amount + " damage to";
        }

        /*
        string text = "";
        if (effectType == EffectType.HEAL_DAMAGE)
        {
            text += plural ? "heals " : "heal ";
        }
        else
        {
            text += plural ? "takes " : "take ";
        }
        text += amount.ToString() + " damage";
        return text;
        */
    }
    public override Alignment GetAlignment()
    {
        return (effectType == EffectType.HEAL_DAMAGE) ? Alignment.POSITIVE : Alignment.NEGATIVE;
    }
    public override double PowerLevel()
    {
        return ((effectType == EffectType.HEAL_DAMAGE) ? 0.5 : 1.0) * (amount - 0.5) * PowerBudget.UNIT_COST;
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
        desc.amount = random.Next(min, max + 1);

        return desc;
    }

    public override IEffectDescription GetDescriptionType()
    {
        return new DamageEffectDescription(heal);
    }

    public override double GetMinCost()
    {
        DamageEffectDescription desc = new DamageEffectDescription(heal);
        desc.amount = 1;
        return desc.PowerLevel();
    }
}
