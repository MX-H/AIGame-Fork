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
    NONE            = 0,
    CREATURE        = 1 << 0,
    SPELL           = 1 << 1,
    TRAP            = 1 << 2,

    // Cost based
    LOW_COST        = 1 << 3,
    MID_COST        = 1 << 4,
    HIGH_COST       = 1 << 5,

    // Reserve 10 spots for creature types
    HUMAN           = 1 << 6,
    GOBLIN          = 1 << 7,
    FAERIE          = 1 << 8,

    // Reserve 10 spots for keywords
    EVASION         = 1 << 16,
    FAST_STRIKE     = 1 << 17,
    UNTOUCHABLE     = 1 << 18,
    PIERCING        = 1 << 19,
    EAGER           = 1 << 20,

    // Targetting types
    SINGLE_TARGET   = 1 << 26,
    MULTI_TARGET    = 1 << 27,
    AOE             = 1 << 28,
}

// Try not to reorder any existing flags, we will need to retag everything if that happens
[Flags]
public enum WordTags
{
    NONE                = 0,

    // Types of nouns
    NOUN_PROPER                 = 1 << 1,
    NOUN_CONCRETE               = 1 << 2,
    NOUN_ABSTRACT               = 1 << 3,
    NOUN_COLLECTIVE             = 1 << 4,
    NOUN_ATTRIBUTIVE            = 1 << 5,

    // This is a seperate noun flag, each noun is countable or uncountable
    NOUN_UNCOUNTABLE              = 1 << 7,
    NOUN_PLURAL                 = 1 << 8,
    NOUN_SINGULAR               = 1 << 9,
    
    // Types of adjectives
    ADJ_DESCRIPTIVE             = 1 << 10,
    ADJ_QUANTITATIVE_CARDINAL   = 1 << 11,
    ADJ_QUANTITATIVE_ORDINAL    = 1 << 12,
    ADJ_PROPER                  = 1 << 13,

    // For the names of cards we only need to use non-finite action verbs ie. flying goblin
    VERB_ACTION                 = 1 << 20,
    VERB_GERUND                 = 1 << 21,
    VERB_ADJ                    = 1 << 22,

    ADVERB_FREQUENCY            = 1 << 26,
    ADVERB_DEGREE               = 1 << 27,
    ADVERB_MANNER               = 1 << 28,
}

public static class CardTagging
{
    public static CardTags GetCardTags(CardDescription cardDesc)
    {
        CardTags tags = GetCardTagsFromCardType(cardDesc.cardType);
        tags |= GetCardTagsFromManaCost(cardDesc.manaCost);
        switch (cardDesc.cardType)
        {
            case CardType.CREATURE:
                CreatureCardDescription creatureDesc = cardDesc as CreatureCardDescription;
                tags |= GetCardTagsFromCreatureType(creatureDesc.creatureType);
                foreach (KeywordAttribute keywords in creatureDesc.GetAttributes())
                {
                    tags |= GetCardTagsFromKeywordAttributes(keywords);
                }

                break;
            case CardType.SPELL:
            case CardType.TRAP:
                foreach (CardEffectDescription effect in cardDesc.cardEffects)
                {
                    if (effect.GetAlignment() == Alignment.POSITIVE)
                    {
                        tags |= GetCardTagsFromTargetType(effect.targettingType);
                    }
                }

                break;
        }
        return tags;
    }

    public static CardTags GetCardTagsFromCardType(CardType cardType)
    {
        switch (cardType)
        {
            case CardType.CREATURE:
                return CardTags.CREATURE;
            case CardType.SPELL:
                return CardTags.SPELL;
            case CardType.TRAP:
                return CardTags.TRAP;
        }
        return CardTags.NONE;
    }

    public static CardTags GetCardTagsFromCreatureType(CreatureType creatureType)
    {
        switch (creatureType)
        {
            case CreatureType.HUMAN:
                return CardTags.HUMAN;
            case CreatureType.GOBLIN:
                return CardTags.GOBLIN;
            case CreatureType.FAERIE:
                return CardTags.FAERIE;
        }
        return CardTags.NONE;
    }

    public static CardTags GetCardTagsFromKeywordAttributes(KeywordAttribute keyword)
    {
        switch (keyword)
        {
            case KeywordAttribute.EVASION:
                return CardTags.EVASION;
            case KeywordAttribute.FAST_STRIKE:
                return CardTags.FAST_STRIKE;
            case KeywordAttribute.UNTOUCHABLE:
                return CardTags.UNTOUCHABLE;
            case KeywordAttribute.PIERCING:
                return CardTags.PIERCING;
            case KeywordAttribute.EAGER:
                return CardTags.EAGER;
        }
        return CardTags.NONE;
    }

    public static CardTags GetCardTagsFromManaCost(int mana)
    {
        if (mana < 4)
        {
            return CardTags.LOW_COST;
        }
        else if (mana < 7)
        {
            return CardTags.MID_COST;
        }
        else
        {
            return CardTags.HIGH_COST;
        }
    }

    public static CardTags GetCardTagsFromTargetType(ITargettingDescription targettingDesc)
    {
        switch(targettingDesc.targettingType)
        {
            case TargettingType.TARGET:
                if (targettingDesc is TargetXDescription targetDesc)
                {
                    if (targetDesc.amount == 1)
                    {
                        return CardTags.SINGLE_TARGET;
                    }
                    else
                    {
                        return CardTags.MULTI_TARGET;
                    }
                }
                break;
            case TargettingType.UP_TO_TARGET:
                if (targettingDesc is UpToXTargetDescription upToTargetDesc)
                {
                    if (upToTargetDesc.amount == 1)
                    {
                        return CardTags.SINGLE_TARGET;
                    }
                    else
                    {
                        return CardTags.MULTI_TARGET;
                    }
                }
                break;
            case TargettingType.ALL:
                return CardTags.AOE;
        }
        return CardTags.NONE;
    }

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
    public static readonly double DOWNSIDE_WEIGHT = 1.25;
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
            case KeywordAttribute.EAGER:
                return UNIT_COST * 0.75;
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
                return (card is CreatureCardDescription) ? 2.5 : 1.0;
            // Should occur very frequently, and creature entering is very hard for opponent to play around
            // Entering is more threatening than dying
            case TriggerCondition.ON_CREATURE_ENTER:
                return (card is CreatureCardDescription) ? 3 : 1.0;
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
                        multiplier += 0.5 * Math.Min(5, creatureCard.attack) / 5;
                    }
                    if (creatureCard.HasKeywordAttribute(KeywordAttribute.FAST_STRIKE))
                    {
                        // Fast strike is less appealing to block with high power, like piercing but will not get damage through
                        // a blocker only deter opponent from blocking so cap is smaller
                        multiplier += 0.25 * Math.Min(5, creatureCard.attack) / 5;
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