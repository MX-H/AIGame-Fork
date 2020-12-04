using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Flags]
public enum CardGenerationFlags
{
    NONE = 0,
    CREATURE = 1 << 0,
    SPELL = 1 << 1,
    TRAP = 1 << 2,

    HUMAN = 1 << 3,
    GOBLIN = 1 << 4,
    FAERIE = 1 << 5,
}

[Flags]
public enum CardTags
{
    NONE = 0,
    CREATURE = 1 << 0,
    SPELL = 1 << 1,
    TRAP = 1 << 2,

    HUMAN = 1 << 3,
    GOBLIN = 1 << 4,
    FAERIE = 1 << 5,

    EVASION = 1 << 6
}

public static class EffectConstants
{
    public static readonly int MAX_CARD_EFFECTS = 5;

    public static CardGenerationFlags GetGenerationFlags(CardType cardType)
    {
        switch (cardType)
        {
            case CardType.CREATURE:
                return CardGenerationFlags.CREATURE;
            case CardType.SPELL:
                return CardGenerationFlags.SPELL;
            case CardType.TRAP:
                return CardGenerationFlags.TRAP;
        }
        return CardGenerationFlags.NONE;
    }

    public static CardGenerationFlags GetGenerationFlags(CreatureType creatureType)
    {
        CardGenerationFlags flag = CardGenerationFlags.CREATURE;
        switch (creatureType)
        {
            case CreatureType.HUMAN:
                flag |= CardGenerationFlags.HUMAN;
                break;
            case CreatureType.GOBLIN:
                flag |= CardGenerationFlags.GOBLIN;
                break;
            case CreatureType.FAERIE:
                flag |= CardGenerationFlags.FAERIE;
                break;
        }
        return flag;
    }
}
public static class PowerBudget
{
    public static readonly double UNIT_COST = 1.0;
    public static readonly double DOWNSIDE_WEIGHT = 1.2;
    public static readonly double FLAT_EFFECT_COST = 0.5;

    public static double ManaFunction(double mana)
    {
        return (-1.0 / 90.0) * mana * mana + (91.0 / 90.0) * mana + 1.5;
    }

    public static int StatBudget(double mana)
    {
        return (int)Math.Round((mana + 0.5) * 2.0, MidpointRounding.AwayFromZero);
    }

    public static int PowerLevelToMana(double powerLevel)
    {
        // quadratic formula
        double a = -1.0 / 90.0;
        double b = 91.0 / 90.0;
        double c = 1.5 - powerLevel;

        double discriminant = b * b - 4 * a * c;
        if (discriminant < 0)
        {
            // This means that you have a power level higher than the mana function could have produced
            // This should not be an issue, because the max is at around 45 mana
            return 10;
        }

        double x1 = (-b + Math.Sqrt(discriminant)) / (2 * a);
        double x2 = (-b + Math.Sqrt(discriminant)) / (2 * a);
        double x = Math.Min(x1, x2);

        if (x < 0)
        {
            x = 0;
        }
        else if (x > 10)
        {
            x = 10;
        }

        return (int)Math.Round(x, MidpointRounding.AwayFromZero);
    }

    public static double StatsToPowerBudget(int statTotal)
    {
        double manaCost = (statTotal / 2.0) - 0.5;
        return ManaFunction(manaCost);
    }

    public static readonly double[] ManaPowerBudgets = new double[]
    {
        ManaFunction(0) * UNIT_COST,
        ManaFunction(1) * UNIT_COST,
        ManaFunction(2) * UNIT_COST,
        ManaFunction(3) * UNIT_COST,
        ManaFunction(4) * UNIT_COST,
        ManaFunction(5) * UNIT_COST,
        ManaFunction(6) * UNIT_COST,
        ManaFunction(7) * UNIT_COST,
        ManaFunction(8) * UNIT_COST,
        ManaFunction(9) * UNIT_COST,
        ManaFunction(10) * UNIT_COST,
    };

    public static readonly double[] ManaPowerMargin = new double[]
    {
            ManaFunction(0.5) * UNIT_COST,
            ManaFunction(1.5) * UNIT_COST,
            ManaFunction(2.5) * UNIT_COST,
            ManaFunction(3.5) * UNIT_COST,
            ManaFunction(4.5) * UNIT_COST,
            ManaFunction(5.5) * UNIT_COST,
            ManaFunction(6.5) * UNIT_COST,
            ManaFunction(7.5) * UNIT_COST,
            ManaFunction(8.5) * UNIT_COST,
            ManaFunction(9.5) * UNIT_COST,
            ManaFunction(10.5) * UNIT_COST,
    };

    public static readonly double[] ManaPowerLimit = new double[]
    {
            ManaFunction(1) * UNIT_COST,
            ManaFunction(2) * UNIT_COST,
            ManaFunction(3) * UNIT_COST,
            ManaFunction(4) * UNIT_COST,
            ManaFunction(5) * UNIT_COST,
            ManaFunction(6) * UNIT_COST,
            ManaFunction(7) * UNIT_COST,
            ManaFunction(8) * UNIT_COST,
            ManaFunction(9) * UNIT_COST,
            ManaFunction(10) * UNIT_COST,
            ManaFunction(11) * UNIT_COST,
    };

    public static double GetKeywordCost(KeywordAttribute keyword, int atk, int def)
    {
        switch (keyword)
        {
            case KeywordAttribute.NONE:
                return 0;
            case KeywordAttribute.EVASION:
                return (atk / 2.0 + 0.5) * UNIT_COST;
            case KeywordAttribute.FAST_STRIKE:
                return atk / 3.0 * UNIT_COST;
            case KeywordAttribute.PIERCING:
                return 0.5 * (atk / 2.0 + def / 3.0) * UNIT_COST;
            case KeywordAttribute.UNTOUCHABLE:
                return UNIT_COST;
        }

        // If we have not defined a cost function return a really high cost so we don't generate this effect
        Debug.Log("Cost not defined for " + keyword.ToString());
        return ManaFunction(20) * UNIT_COST;
    }

    public static double GetTriggerModifier(TriggerCondition trigger, CardDescription card)
    {
        switch (trigger)
        {
            // Can happen repeatedly and can make decisions very difficult for opponent
            case TriggerCondition.ON_CREATURE_DIES:
                return (card is CreatureCardDescription) ? 1.5 : 1.0;
            // Should occur very frequently, and creature entering is very hard for opponent to play around
            // Entering is more threatening than dying
            case TriggerCondition.ON_CREATURE_ENTER:
                return (card is CreatureCardDescription) ? 2.0 : 1.0;
            // Worse than on enter, on self enter is like a spell so its the baseline for an effects strength
            case TriggerCondition.ON_SELF_DIES:
                return 0.9;
            // Very good on bodies with good offensive keywords
            case TriggerCondition.ON_SELF_DAMAGE_DEALT_TO_PLAYER:
                {
                    CreatureCardDescription creatureCard = card as CreatureCardDescription;
                    // Base is worse than a mostly guarenteed trigger like on dies
                    // Gets better the easier it is to inflict player damage

                    // Much weaker with a 0 power creature, because you require another card or effect to help
                    double multiplier = (creatureCard.attack == 0) ? 0.25 : 0.5;
                    if (creatureCard.HasKeywordAttribute(KeywordAttribute.EVASION))
                    {
                        // Evasion is the strongest offensive keyword
                        multiplier += (creatureCard.attack == 0) ? 0.5 : 1;
                    }
                    if (creatureCard.HasKeywordAttribute(KeywordAttribute.PIERCING))
                    {
                        // Piercing depends on how good the body is, for now we'll say the average blocker has 4 toughness,
                        // So you get the max effectiveness from piercing at 5 attack
                        multiplier += 0.5 * Math.Max(5, creatureCard.attack);
                    }
                    if (creatureCard.HasKeywordAttribute(KeywordAttribute.FAST_STRIKE))
                    {
                        // Fast strike is less appealing to block with high power, like piercing but will not get damage through
                        // a blocker only deter opponent from blocking so cap is smaller
                        multiplier += 0.25 * Math.Max(5, creatureCard.attack);
                    }
                    if (creatureCard.HasKeywordAttribute(KeywordAttribute.EAGER))
                    {
                        // With eager the player can likely guarentee the effect at least once
                        if (creatureCard.attack > 0)
                        {
                            multiplier += 0.5;
                        }
                    }

                    return multiplier;
                }
            // Good with high health, also good with untouchable because the common way to beat it is through combat
            case TriggerCondition.ON_SELF_DAMAGE_TAKEN:
                {
                    CreatureCardDescription creatureCard = card as CreatureCardDescription;
                    // Assume average damage will come in 3s, but cap how strong the card effect multiplier is
                    double multiplier = Math.Max(3.0, creatureCard.health / 3.0);
                    if (!creatureCard.HasKeywordAttribute(KeywordAttribute.UNTOUCHABLE))
                    {
                        multiplier *= 0.75;
                    }

                    return multiplier;
                }
        }

        return 1.0;
    }
}