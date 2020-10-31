using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class DrawEffectDescription : IEffectDescription
{
    public DrawModifier drawModifier;
    public int amount;
    public DrawEffectDescription() : base(EffectType.DRAW_CARDS)
    {
        drawModifier = DrawModifier.SELF;
    }

    public override void ApplyToTarget(Targettable target, PlayerController player)
    {
        PlayerController targetPlayer = target as PlayerController;
        if (targetPlayer)
        {
            GameSession gameSession = GameUtils.GetGameSession();
            for (int i = 0; i < amount; i++)
            {
                gameSession.ServerPlayerDrawCard(targetPlayer);
            }
        }
    }

    public override string CardText(bool plural) 
    {
        return (plural ? "draws " : "draw ") + amount.ToString() + ((amount == 1) ? " card" : " cards");
    }

    public override Alignment GetAlignment()
    {
        return Alignment.POSITIVE;
    }

    public override double PowerLevel()
    {
        return (amount - 0.5) * 2 * PowerBudget.UNIT_COST;
    }
}


public class DrawEffectProceduralGenerator : IProceduralEffectGenerator
{
    private static int MIN_CARDS = 1;
    private static int MAX_CARDS = 5;
    public override IEffectDescription Generate()
    {
        DrawEffectDescription desc = new DrawEffectDescription();
        desc.drawModifier = ProceduralUtils.GetRandomValue<DrawModifier>(random, model);

        // Find the bounds of card amounts
        int max = ProceduralUtils.GetUpperBound(desc, ref desc.amount, MIN_CARDS, MAX_CARDS, maxAllocatedBudget);
        int min = ProceduralUtils.GetLowerBound(desc, ref desc.amount, MIN_CARDS, max, minAllocatedBudget);

        Assert.IsTrue(max >= min);
        desc.amount = random.Next(min, max);

        return desc;
    }

    public override IEffectDescription GetDescriptionType()
    {
        return new DrawEffectDescription();
    }

    public override double GetMinCost()
    {
        DrawEffectDescription desc = new DrawEffectDescription();
        desc.amount = 1;
        return desc.PowerLevel();
    }
}