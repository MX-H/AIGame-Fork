using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class DrawEffectDescription : IEffectDescription
{
    public DrawModifier drawModifier;
    public int amount;
    public IQualifierDescription cardQualifier;
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

            CardGenerationFlags flags = CardGenerationFlags.NONE;
            if (cardQualifier != null)
            {
                switch (cardQualifier.qualifierType)
                {
                case QualifierType.CARD_TYPE:
                    {
                        CardTypeQualifierDescription cardTypeQualifier = cardQualifier as CardTypeQualifierDescription;
                        flags |= EffectConstants.GetGenerationFlags(cardTypeQualifier.cardType);
                    }
                    break;
                case QualifierType.CREATURE_TYPE:
                    {
                        CreatureTypeQualifierDescription creatureTypeQualifier = cardQualifier as CreatureTypeQualifierDescription;
                        flags |= EffectConstants.GetGenerationFlags(creatureTypeQualifier.creatureType);
                    }
                    break;
                }
            }

            for (int i = 0; i < amount; i++)
            {
                switch (drawModifier)
                {
                    case DrawModifier.SELF:
                    case DrawModifier.RANDOM:
                        gameSession.ServerPlayerDrawCard(targetPlayer, targetPlayer, flags);
                        break;
                    case DrawModifier.OPPONENT_RANDOM:
                    case DrawModifier.OPPONENT:
                        gameSession.ServerPlayerDrawCard(targetPlayer, GameUtils.GetGameSession().GetOpponents(targetPlayer)[0], flags);
                        break;
                }
            }
        }
    }

    public override string CardText(bool plural) 
    {
        string text = (plural ? "draws " : "draw ") + amount.ToString();

        if (cardQualifier != null)
        {
            switch (cardQualifier.qualifierType)
            {
            case QualifierType.CARD_TYPE:
                {
                    CardTypeQualifierDescription cardTypeQualifier = cardQualifier as CardTypeQualifierDescription;
                    text += " " + CardParsing.Parse(cardTypeQualifier.cardType);
                }
                break;
            case QualifierType.CREATURE_TYPE:
                {
                    CreatureTypeQualifierDescription creatureTypeQualifier = cardQualifier as CreatureTypeQualifierDescription;
                    text += " " + CardParsing.Parse(creatureTypeQualifier.creatureType);
                }
                break;
            }
        }

        text += ((amount == 1) ? " card" : " cards");

        switch (drawModifier)
        {
            case DrawModifier.OPPONENT:
                text += " from the other player's deck";
                break;
            case DrawModifier.OPPONENT_RANDOM:
                text += " from the other player's deck";
                break;
        }

        return text;
    }

    public override Alignment GetAlignment()
    {
        return Alignment.POSITIVE;
    }

    public override double PowerLevel()
    {
        return amount * 1.5 * PowerBudget.UNIT_COST;
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

        // Attempt to narrow down the qualifier pool
        SortedSet<QualifierType> allowableQualifiers = CardEnums.GetValidFlags<QualifierType>(EffectType.DRAW_CARDS);

        QualifierType qualifier = ProceduralUtils.GetRandomValue(random, model, allowableQualifiers);
        if (qualifier != QualifierType.NONE)
        {
            IProceduralQualifierGenerator qualifierGen = ProceduralUtils.GetProceduralGenerator(qualifier);
            qualifierGen.SetupParameters(random, model, minAllocatedBudget / desc.PowerLevel(), maxAllocatedBudget / desc.PowerLevel());
            desc.cardQualifier = qualifierGen.Generate();
        }

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