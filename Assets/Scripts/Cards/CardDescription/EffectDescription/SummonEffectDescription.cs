using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class SummonEffectDescription : IEffectDescription
{
    public int amount;
    public CreatureType tokenType;
    public int manaCost;
    public SummonEffectDescription() : base(EffectType.SUMMON_TOKEN)
    { }

    public override void ApplyToTarget(Targettable target, PlayerController player)
    {
        PlayerController targetPlayer = target as PlayerController;
        if (targetPlayer)
        {
            GameSession gameSession = GameUtils.GetGameSession();
            for (int i = 0; i < amount; i++)
            {
                gameSession.ServerCreateToken(targetPlayer, tokenType);
            }
        }
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
        // Tokens tend to suck
        return (amount - 0.5) * (manaCost) * PowerBudget.UNIT_COST;
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
        desc.manaCost = creatureModelIndex.GetToken(desc.tokenType).manaCost;

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
        // We are assuming that on average a token is 1 mana of value
        // This is a bit flawed because the token type is randomly chosen with a probability model
        // so the cost of the token is not determined by the budget
        SummonEffectDescription desc = new SummonEffectDescription();
        desc.amount = 1;
        desc.manaCost = 1;
        return desc.PowerLevel();
    }
}