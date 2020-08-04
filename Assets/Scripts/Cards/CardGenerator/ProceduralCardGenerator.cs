using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class ProceduralCardGenerator : ICardGenerator
{
    private IHistogram model;
    public ProceduralCardGenerator(IHistogram m)
    {
        model = m;
    }

    public override CardDescription GenerateCard(int seed)
    {
        // First select the card type
        System.Random random = new System.Random(seed);
        CardType t = ProceduralUtils.GetRandomValue<CardType>(random, model);

        switch (t)
        {
            case CardType.CREATURE:
                return GenerateCreatureCard(random, model);
            case CardType.SPELL:
                return GenerateSpellCard(random, model);
            case CardType.TRAP:
                return GenerateTrapCard(random, model);
        }

        return new CardDescription();
    }

    static private CardDescription GenerateCreatureCard(System.Random random, IHistogram model)
    {
        CreatureCardDescription card = new CreatureCardDescription();
        card.creatureType = ProceduralUtils.GetRandomValue<CreatureType>(random, model);
        card.manaCost = (int)ProceduralUtils.GetRandomValue<ManaCost>(random, model);

        // Decide on power budget
        double powerBudget = PowerBudget.ManaPowerBudgets[card.manaCost];
        double powerMargin = PowerBudget.ManaPowerMargin[card.manaCost];
        double powerLimit = PowerBudget.ManaPowerLimit[card.manaCost];
        card.name = "A creature card (" + powerBudget.ToString() + ")";

        // Decide on stats
        card.attack = random.Next(card.manaCost);
        card.health = card.manaCost - card.attack + 1;

        // Decide on keyword attributes
        int amount = random.Next(2);
        for (int i = 0; i < amount; i++)
        {
            card.attributes.Add(ProceduralUtils.GetRandomValue<KeywordAttribute>(random, model));
        }

        // Decide on effects
        GenerateCardEffects(random, model, card, powerBudget, powerMargin, powerLimit);

        return card;
    }

    static private CardDescription GenerateSpellCard(System.Random random, IHistogram model)
    {
        CardDescription card = new CardDescription();
        card.cardType = CardType.SPELL;
        card.manaCost = (int)ProceduralUtils.GetRandomValue<ManaCost>(random, model);

        double powerBudget = PowerBudget.ManaPowerBudgets[card.manaCost];
        double powerMargin = PowerBudget.ManaPowerMargin[card.manaCost];
        double powerLimit = PowerBudget.ManaPowerLimit[card.manaCost];
        card.name = "A spell card(" + powerBudget.ToString() + ")";

        GenerateCardEffects(random, model, card, powerBudget, powerMargin, powerLimit);

        return card;
    }

    static private CardDescription GenerateTrapCard(System.Random random, IHistogram model)
    {
        CardDescription card = new CardDescription();
        card.cardType = CardType.TRAP;
        card.manaCost = (int)ProceduralUtils.GetRandomValue<ManaCost>(random, model);

        double powerBudget = PowerBudget.ManaPowerBudgets[card.manaCost];
        double powerMargin = PowerBudget.ManaPowerMargin[card.manaCost];
        double powerLimit = PowerBudget.ManaPowerLimit[card.manaCost];
        card.name = "A trap card(" + powerBudget.ToString() + ")";

        GenerateCardEffects(random, model, card, powerBudget, powerMargin, powerLimit);

        return card;
    }

    static private void GenerateCardEffects(System.Random random, IHistogram model, CardDescription cardDesc, double powerBudget, double powerMargin, double powerLimit)
    {
        // A trap card only has one trigger condition
        if (cardDesc.cardType == CardType.TRAP)
        {
            ProceduralUtils.GetRandomValue(random, model, CardEnums.GetValidFlags<TriggerCondition>(CardType.TRAP));
        }

        int effectCount = 0;

        List<TriggerCondition> triggerBlacklist = new List<TriggerCondition>();

        while ((cardDesc.PowerLevel() + PowerBudget.FLAT_EFFECT_COST) < powerBudget && effectCount < EffectConstants.MAX_CARD_EFFECTS)
        {
            // Generate effects
            CardEffectDescription cardEffect = new CardEffectDescription();

            // Create effects with a power level ranging between using max limit of budget, or uniformly distributing the min budget
            double maxAllowableBudget = powerLimit - cardDesc.PowerLevel() - PowerBudget.FLAT_EFFECT_COST;
            double minAllowableBudget = (powerBudget - cardDesc.PowerLevel() - PowerBudget.FLAT_EFFECT_COST) / (EffectConstants.MAX_CARD_EFFECTS - effectCount);

            SortedSet<EffectType> candidatesWithinBudget = ProceduralUtils.GetEffectsWithinBudget(maxAllowableBudget);
            if (candidatesWithinBudget.Count == 0)
            {
                Debug.Log("No effects are valid for budget " + maxAllowableBudget);
                break;
            }

            if (cardDesc.cardType == CardType.SPELL || (cardDesc.cardType == CardType.TRAP && effectCount > 0))
            {
                // After the first condition of a trap or after casting a spell all further effects of
                // the card should just resolve so trigger cond is NONE
                cardEffect.triggerCondition = TriggerCondition.NONE;
            }
            else {
                cardEffect.triggerCondition = ProceduralUtils.GetRandomValueExcluding(random, model, triggerBlacklist, CardEnums.GetValidFlags<TriggerCondition>(cardDesc.cardType));
            }

            bool validEffect = false;

            SortedSet<EffectType> effectCandidates = new SortedSet<EffectType>(CardEnums.GetValidFlags<EffectType>(cardEffect.triggerCondition).Intersect(candidatesWithinBudget));
            effectCandidates.Remove(EffectType.NONE);

            while (!validEffect && effectCandidates.Count > 0)
            {
                EffectType effectType = ProceduralUtils.GetRandomValueExcluding(random, model, new EffectType[] { EffectType.NONE },
                    CardEnums.GetValidFlags<EffectType>(cardEffect.triggerCondition));
                // This means that there isn't an effect that meets this condition
                if (effectType == EffectType.NONE)
                {
                    // Add to black list so we don't get multiple effects that trigger on same condition
                    if (cardEffect.triggerCondition != TriggerCondition.NONE)
                    {
                        triggerBlacklist.Add(cardEffect.triggerCondition);
                    }
                    break;
                }

                validEffect = GenerateCardEffect(random, model, cardEffect, effectType, minAllowableBudget, maxAllowableBudget, true);
            }

            // This means that there isn't an effect that meets this condition
            if (validEffect)
            {
                cardDesc.cardEffects.Add(cardEffect);
                effectCount++;
            }
            else
            {
                Debug.Log("No valid effect could be generated for trigger <" + cardEffect.triggerCondition.ToString() + "> with budget " + maxAllowableBudget);
            }

            // Add to black list so we don't get multiple effects that trigger on same condition
            if (cardEffect.triggerCondition != TriggerCondition.NONE)
            {
                triggerBlacklist.Add(cardEffect.triggerCondition);
            }
        }

        // Generate a draw back
        if (cardDesc.PowerLevel() > powerMargin)
        {
            // TODO: Drawback should be one of, additional cost, negative effect, or negative modification to existing effect
            // For now just add additional effect

            CardEffectDescription cardEffect = new CardEffectDescription();

            double currPower = cardDesc.PowerLevel();

            double maxAllowableBudget = (cardDesc.PowerLevel() - powerBudget - PowerBudget.FLAT_EFFECT_COST) / PowerBudget.DOWNSIDE_WEIGHT;
            double minAllowableBudget = (cardDesc.PowerLevel() - powerMargin - PowerBudget.FLAT_EFFECT_COST) / PowerBudget.DOWNSIDE_WEIGHT;

            bool validEffect = false;


            SortedSet<EffectType> candidatesWithinBudget = ProceduralUtils.GetEffectsWithinBudget(maxAllowableBudget);
            if (candidatesWithinBudget.Count == 0)
            {
                validEffect = true;
                Debug.Log("No effects are valid for budget " + maxAllowableBudget);
            }


            while (!validEffect)
            {
                if (cardDesc.cardType == CardType.SPELL || (cardDesc.cardType == CardType.TRAP))
                {
                    // After the first condition of a trap or after casting a spell all further effects of
                    // the card should just resolve so trigger cond is NONE
                    cardEffect.triggerCondition = TriggerCondition.NONE;
                }
                else
                {
                    cardEffect.triggerCondition = ProceduralUtils.GetRandomValueExcluding(random, model, triggerBlacklist, CardEnums.GetValidFlags<TriggerCondition>(cardDesc.cardType));
                }

                SortedSet<EffectType> effectCandidates = new SortedSet<EffectType>(CardEnums.GetValidFlags<EffectType>(cardEffect.triggerCondition).Intersect(candidatesWithinBudget));
                effectCandidates.Remove(EffectType.NONE);

                while (!validEffect && effectCandidates.Count > 0)
                {
                    EffectType effectType = ProceduralUtils.GetRandomValueExcluding(random, model, new EffectType[] { EffectType.NONE },
                        CardEnums.GetValidFlags<EffectType>(cardEffect.triggerCondition));
                    // This means that there isn't an effect that meets this condition
                    if (effectType == EffectType.NONE)
                    {
                        // Add to black list so we don't get multiple effects that trigger on same condition
                        if (cardEffect.triggerCondition != TriggerCondition.NONE)
                        {
                            triggerBlacklist.Add(cardEffect.triggerCondition);
                        }
                        break;
                    }

                    validEffect = GenerateCardEffect(random, model, cardEffect, effectType, minAllowableBudget, maxAllowableBudget, false);
                }

                // This means that there isn't an effect that meets this condition
                if (validEffect)
                {
                    cardDesc.cardEffects.Add(cardEffect);
                }
                else
                {
                    Debug.Log("No valid effect could be generated for trigger <" + cardEffect.triggerCondition.ToString() + "> with budget " + -maxAllowableBudget);
                }
            }

        }

    }

    static private bool GenerateCardEffect(System.Random random, IHistogram model, CardEffectDescription effectDesc, EffectType effect, double minBudget, double maxBudget, bool positive)
    {
        IProceduralEffectGenerator effectGen = ProceduralUtils.GetProceduralGenerator(effect);
        effectGen.SetupParameters(random, model, minBudget, maxBudget);
        effectDesc.effectType = effectGen.Generate();

        // Adjust budgets
        minBudget /= effectDesc.effectType.PowerLevel();
        maxBudget /= effectDesc.effectType.PowerLevel();
        if (minBudget > maxBudget)
        {
            double temp = minBudget;
            minBudget = maxBudget;
            maxBudget = temp;
        }
        // Always allow for default targetting (multiplier 1.0x)
        if (maxBudget < 1.0)
        {
            maxBudget = 1.0;
        }

        TargetType targetType = TargetType.CREATURES;
        SortedSet<TargetType> validTargets = CardEnums.GetValidFlags<TargetType>(effect);
        SortedSet<TargettingType> allowableTargetting = new SortedSet<TargettingType>();
        SortedSet<QualifierType> allowableQualifiers = new SortedSet<QualifierType>();

        while (validTargets.Count > 0 && allowableTargetting.Count == 0)
        {
            targetType = ProceduralUtils.GetRandomValue(random, model, validTargets);
            validTargets.Remove(targetType);

            switch (effectDesc.effectType.GetAlignment())
            {
                case Alignment.POSITIVE:
                    if (positive)
                    {
                        allowableTargetting = ProceduralUtils.GetTargettingByAlignment(Alignment.POSITIVE);

                        allowableQualifiers = ProceduralUtils.GetQualifiersByAlignment(Alignment.NEUTRAL);
                        allowableQualifiers.UnionWith(ProceduralUtils.GetQualifiersByAlignment(Alignment.POSITIVE));
                        allowableQualifiers.IntersectWith(CardEnums.GetValidFlags<QualifierType>(targetType));
                        if (allowableQualifiers.Count > 0)
                        {
                            allowableTargetting.UnionWith(ProceduralUtils.GetTargettingByAlignment(Alignment.NEUTRAL));
                        }
                    }
                    else
                    {
                        allowableTargetting = ProceduralUtils.GetTargettingByAlignment(Alignment.NEGATIVE);

                        allowableQualifiers = ProceduralUtils.GetQualifiersByAlignment(Alignment.NEGATIVE);
                        allowableQualifiers.IntersectWith(CardEnums.GetValidFlags<QualifierType>(targetType));
                        if (allowableQualifiers.Count > 0)
                        {
                            allowableTargetting.UnionWith(ProceduralUtils.GetTargettingByAlignment(Alignment.NEUTRAL));
                        }
                    }
                    break;
                case Alignment.NEGATIVE:
                    if (positive)
                    {
                        allowableTargetting = ProceduralUtils.GetTargettingByAlignment(Alignment.NEGATIVE);

                        allowableQualifiers = ProceduralUtils.GetQualifiersByAlignment(Alignment.NEUTRAL);
                        allowableQualifiers.UnionWith(ProceduralUtils.GetQualifiersByAlignment(Alignment.NEGATIVE));
                        allowableQualifiers.IntersectWith(CardEnums.GetValidFlags<QualifierType>(targetType));
                        if (allowableQualifiers.Count > 0)
                        {
                            allowableTargetting.UnionWith(ProceduralUtils.GetTargettingByAlignment(Alignment.NEUTRAL));
                        }
                    }
                    else
                    {
                        allowableTargetting = ProceduralUtils.GetTargettingByAlignment(Alignment.POSITIVE);
                        allowableQualifiers = ProceduralUtils.GetQualifiersByAlignment(Alignment.POSITIVE);
                        allowableQualifiers.IntersectWith(CardEnums.GetValidFlags<QualifierType>(targetType));
                        if (allowableQualifiers.Count > 0)
                        {
                            allowableTargetting.UnionWith(ProceduralUtils.GetTargettingByAlignment(Alignment.NEUTRAL));
                        }
                    }
                    break;
                default:
                    if (positive)
                    {
                        allowableTargetting = new SortedSet<TargettingType>((TargettingType[])Enum.GetValues(typeof(TargettingType)));
                        allowableQualifiers = new SortedSet<QualifierType>((QualifierType[])Enum.GetValues(typeof(QualifierType)));
                        allowableQualifiers.IntersectWith(CardEnums.GetValidFlags<QualifierType>(targetType));
                    }
                    else
                    {
                        allowableTargetting = new SortedSet<TargettingType>();
                    }
                    break;
            }

            allowableTargetting.IntersectWith(CardEnums.GetValidFlags<TargettingType>(targetType));

            // Special case
            // Up to can never be a downside because you can choose 0 targets
            if (!positive)
            {
                allowableTargetting.Remove(TargettingType.UP_TO_TARGET);
            }
        }

        // Could not find any valid targetting to achieve the desired alignment
        if (allowableTargetting.Count == 0)
        {
            SortedSet<TargetType> targets = CardEnums.GetValidFlags<TargetType>(effect);

            Debug.Log("Wasn't able to generate targets for effect <" + effect.ToString() + ">");
            return false;
        }


        // Attempt to narrow down the targetting pool
        SortedSet<TargettingType> targettingWithinBudget = new SortedSet<TargettingType>(allowableTargetting.Intersect(ProceduralUtils.GetTargettingWithinBudget(maxBudget)));
        if (targettingWithinBudget.Count > 0)
        {
            allowableTargetting = targettingWithinBudget;
        }
        else
        {
            Debug.Log("Unable to narrow down targetting types for <" + effect.ToString() + ", "  + targetType.ToString() + "> for budget " + maxBudget);
        }

        TargettingType targettingType = ProceduralUtils.GetRandomValue(random, model, allowableTargetting);
        IProceduralTargettingGenerator targettingGen = ProceduralUtils.GetProceduralGenerator(targettingType);
        targettingGen.SetupParameters(targetType, random, model, minBudget, maxBudget);
        effectDesc.targettingType = targettingGen.Generate();
        
        // Adjust budgets
        minBudget /= effectDesc.targettingType.PowerLevel();
        maxBudget /= effectDesc.targettingType.PowerLevel();
        if (minBudget > maxBudget)
        {
            double temp = minBudget;
            minBudget = maxBudget;
            maxBudget = temp;
        }


        if (effectDesc.targettingType is IQualifiableTargettingDescription qualifiable)
        {
            // Generate a possible qualifier

            // Attempt to narrow down the qualifier pool
            SortedSet<QualifierType> qualifiersWithinBudget = new SortedSet<QualifierType>(allowableQualifiers.Intersect(ProceduralUtils.GetQualifiersWithinBudget(maxBudget)));
            if (targettingWithinBudget.Count > 0)
            {
                allowableQualifiers = qualifiersWithinBudget;
            }
            else
            {
                Debug.Log("Unable to narrow down qualifier types for <" + effect.ToString() + ", " + targetType.ToString() + "> for budget " + maxBudget);
            }

            QualifierType qualifier = ProceduralUtils.GetRandomValue(random, model, allowableQualifiers);
            if (qualifier != QualifierType.NONE)
            {
                IProceduralQualifierGenerator qualifierGen = ProceduralUtils.GetProceduralGenerator(qualifier);
                qualifierGen.SetupParameters(random, model, minBudget, maxBudget);
                qualifiable.qualifier = qualifierGen.Generate();
            }
        }

        return true;
    }
}
