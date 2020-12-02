using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class ProceduralCardGenerator : ICardGenerator
{
    private IHistogram model;
    private ImageGlossary images;
    private CreatureModelIndex creatureModelIndex;
    public ProceduralCardGenerator(IHistogram m, ImageGlossary i, CreatureModelIndex creatureModels)
    {
        model = m;
        images = i;
        creatureModelIndex = creatureModels;
    }

    public override CardDescription GenerateCard(int seed)
    {
        // First select the card type
        System.Random random = new System.Random(seed);
        CardType t = ProceduralUtils.GetRandomValue<CardType>(random, model);

        switch (t)
        {
            case CardType.CREATURE:
                return GenerateCreatureCard(random, model, images, creatureModelIndex);
            case CardType.SPELL:
                return GenerateSpellCard(random, model, images, creatureModelIndex);
            case CardType.TRAP:
                return GenerateTrapCard(random, model, images, creatureModelIndex);
        }

        return new CardDescription();
    }

    static private int GeneratePoisson(double lambda, System.Random random)
    {
        // Algorithm due to Donald Knuth, 1969.
        double p = 1.0, L = Math.Exp(-lambda);
        int k = 0;
        do
        {
            k++;
            p *= random.NextDouble();
        }
        while (p > L);
        return k - 1;
    }

    // Generate what mana cost should be allocated to stats
    static private double GetCreatureBodyBudget(double lambda, int seed, int max)
    {
        System.Random rand = new System.Random(seed);
        // We want to generate the body at mana costs of 0.5, poisson only returns ints so sample at double lambda

        int k = GeneratePoisson(lambda * 2, rand);
        while (k > (max * 2))
        {
            k = GeneratePoisson(lambda * 2, rand);
        }

        return k / 2.0;

    }

    static private int GetHealth(System.Random random, CreatureModelIndex.StatProbability probability, int stats)
    {
        int ret = 1;
        int totalCount = 0;
        for (int i = 0; i < stats; i++)
        {
            totalCount += probability.statModels[i].value;
        }

        if (totalCount > 0)
        {
            int result = random.Next(totalCount);
            foreach (CreatureModelIndex.StatModelEntry entry in probability.statModels)
            {
                result -= entry.value;
                if (result < 0)
                {
                    ret = entry.health;
                    break;
                }
            }
        }

        return ret;
    }

    static private CardDescription GenerateCreatureCard(System.Random random, IHistogram model, ImageGlossary images, CreatureModelIndex creatureModels)
    {
        CreatureCardDescription card = ScriptableObject.CreateInstance(typeof(CreatureCardDescription)) as CreatureCardDescription;
        card.creatureType = ProceduralUtils.GetRandomValue<CreatureType>(random, model);

        MultiCardHistogram combinedModel = ScriptableObject.CreateInstance(typeof(MultiCardHistogram)) as MultiCardHistogram;
        combinedModel.Init(new IHistogram[] { model, creatureModels.GetModel(card.creatureType) });

        card.manaCost = (int)ProceduralUtils.GetRandomValue<ManaCost>(random, model);

        // Potentially generate a stronger body, but with a drawback
        double bodyManaCost = GetCreatureBodyBudget(creatureModels.GetBodyLambda(card.creatureType), random.Next(), card.manaCost + 1);
        int maxStats = PowerBudget.StatBudget(bodyManaCost);

        // Decide on stats
        card.health = GetHealth(random, creatureModels.GetStatProbability(card.creatureType, (int)Math.Round(bodyManaCost, MidpointRounding.AwayFromZero)), maxStats);
        card.attack = maxStats - card.health;

        // Decide on power budget
        double powerBudget = PowerBudget.ManaPowerBudgets[card.manaCost];
        double powerMargin = PowerBudget.ManaPowerMargin[card.manaCost];
        double powerLimit = PowerBudget.ManaPowerLimit[card.manaCost];
        card.cardName = "A creature card";
        //card.name += "(" + powerBudget.ToString() + ")";

        // Decide on keyword attributes
        double keywordPowerLimit = powerLimit - card.PowerLevel();
        if (keywordPowerLimit < 0)
        {
            keywordPowerLimit = 0;
        }
        int maxKeywords = 3;
        for (int i = 0; i < maxKeywords; i++)
        {
            KeywordAttribute keyword = ProceduralUtils.GetRandomValue(random, combinedModel, ProceduralUtils.GetKeywordsWithinBudget(keywordPowerLimit, card.attack, card.health));
            if (keyword == KeywordAttribute.NONE)
            {
                break;
            }
            card.attributes.Add(keyword);
        }

        // Decide on effects
        GenerateCardEffects(random, combinedModel, creatureModels, card, powerBudget, powerMargin, powerLimit);

        // Revise the mana cost based on what effects we actually did generate
        int revisedMana = PowerBudget.PowerLevelToMana(card.PowerLevel());
        if (revisedMana != card.manaCost)
        {
            Debug.Log("Had to revise the mana cost from " + card.manaCost.ToString() + " to " + revisedMana.ToString());
            card.manaCost = revisedMana;
        }
        card.image = ProceduralUtils.GetRandomTexture(random, images.GetCreatureImages(card.creatureType));

        return card;
    }

    static private CardDescription GenerateSpellCard(System.Random random, IHistogram model, ImageGlossary images, CreatureModelIndex creatureModels)
    {
        CardDescription card = ScriptableObject.CreateInstance(typeof(CardDescription)) as CardDescription;
        card.cardType = CardType.SPELL;
        card.manaCost = (int)ProceduralUtils.GetRandomValue<ManaCost>(random, model);

        double powerBudget = PowerBudget.ManaPowerBudgets[card.manaCost];
        double powerMargin = PowerBudget.ManaPowerMargin[card.manaCost];
        double powerLimit = PowerBudget.ManaPowerLimit[card.manaCost];
        card.cardName = "A spell card"; 
        //card.name += "(" + powerBudget.ToString() + ")";

        GenerateCardEffects(random, model, creatureModels, card, powerBudget, powerMargin, powerLimit);

        // Revise the mana cost based on what effects we actually did generate
        int revisedMana = PowerBudget.PowerLevelToMana(card.PowerLevel());
        if (revisedMana != card.manaCost)
        {
            Debug.Log("Had to revise the mana cost from " + card.manaCost.ToString() + " to " + revisedMana.ToString());
            card.manaCost = revisedMana;
        }
        card.image = ProceduralUtils.GetRandomTexture(random, images.GetSpellImages());

        return card;
    }

    static private CardDescription GenerateTrapCard(System.Random random, IHistogram model, ImageGlossary images, CreatureModelIndex creatureModels)
    {
        CardDescription card = ScriptableObject.CreateInstance(typeof(CardDescription)) as CardDescription;
        card.cardType = CardType.TRAP;
        card.manaCost = (int)ProceduralUtils.GetRandomValue<ManaCost>(random, model);

        double powerBudget = PowerBudget.ManaPowerBudgets[card.manaCost];
        double powerMargin = PowerBudget.ManaPowerMargin[card.manaCost];
        double powerLimit = PowerBudget.ManaPowerLimit[card.manaCost];
        card.cardName = "A trap card";
        //card.name += "(" + powerBudget.ToString() + ")";

        GenerateCardEffects(random, model, creatureModels, card, powerBudget, powerMargin, powerLimit);

        // Revise the mana cost based on what effects we actually did generate
        int revisedMana = PowerBudget.PowerLevelToMana(card.PowerLevel());
        if (revisedMana != card.manaCost)
        {
            Debug.Log("Had to revise the mana cost from " + card.manaCost.ToString() + " to " + revisedMana.ToString());
            card.manaCost = revisedMana;
        }
        card.image = ProceduralUtils.GetRandomTexture(random, images.GetTrapImages());

        return card;
    }

    static private void GenerateCardEffects(System.Random random, IHistogram model, CreatureModelIndex creatureModels, CardDescription cardDesc, double powerBudget, double powerMargin, double powerLimit)
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

            if (cardDesc.cardType == CardType.SPELL || (cardDesc.cardType == CardType.TRAP && effectCount > 0))
            {
                // After the first condition of a trap or after casting a spell all further effects of
                // the card should just resolve so trigger cond is NONE
                cardEffect.triggerCondition = TriggerCondition.NONE;

                // If NONE has been blacklisted that means that there are no candidates for the remaining budget
                if (triggerBlacklist.Contains(TriggerCondition.NONE))
                {
                    Debug.Log("No effects are valid for budget " + maxAllowableBudget);

                    break;
                }
            }
            else
            {
                if (ProceduralUtils.FlagsExist(triggerBlacklist, CardEnums.GetValidFlags<TriggerCondition>(cardDesc.cardType)))
                {
                    cardEffect.triggerCondition = ProceduralUtils.GetRandomValueExcluding(random, model, triggerBlacklist, CardEnums.GetValidFlags<TriggerCondition>(cardDesc.cardType));
                }
                else
                {
                    // No triggers available mean each trigger type has been blacklisted 

                    Debug.Log("No effects are valid for budget " + maxAllowableBudget);

                    break;
                }
            }

            double triggerBudgetModifier = PowerBudget.GetTriggerModifier(cardEffect.triggerCondition, cardDesc);
            maxAllowableBudget /= triggerBudgetModifier;
            minAllowableBudget /= triggerBudgetModifier;

            SortedSet<EffectType> candidatesWithinBudget = ProceduralUtils.GetEffectsWithinBudget(maxAllowableBudget);
            if (candidatesWithinBudget.Count == 0)
            {
                triggerBlacklist.Add(cardEffect.triggerCondition);

                Debug.Log("No effects are valid for budget " + maxAllowableBudget);
                continue;
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

                validEffect = GenerateCardEffect(random, model, creatureModels, cardEffect, effectType, minAllowableBudget, maxAllowableBudget, true);
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
        while (cardDesc.PowerLevel() > powerMargin)
        {
            // TODO: Drawback should be one of, additional cost, negative effect, or negative modification to existing effect
            // For now just add additional negative effect

            CardEffectDescription cardEffect = new CardEffectDescription();

            double maxAllowableBudget = (cardDesc.PowerLevel() - powerBudget - PowerBudget.FLAT_EFFECT_COST) / PowerBudget.DOWNSIDE_WEIGHT;
            double minAllowableBudget = (cardDesc.PowerLevel() - powerMargin - PowerBudget.FLAT_EFFECT_COST) / PowerBudget.DOWNSIDE_WEIGHT;

            if (cardDesc.cardType == CardType.SPELL || (cardDesc.cardType == CardType.TRAP && effectCount > 0))
            {
                // After the first condition of a trap or after casting a spell all further effects of
                // the card should just resolve so trigger cond is NONE
                cardEffect.triggerCondition = TriggerCondition.NONE;

                // If NONE has been blacklisted that means that there are no candidates for the remaining budget
                if (triggerBlacklist.Contains(TriggerCondition.NONE))
                {
                    Debug.Log("No effects are valid for budget " + maxAllowableBudget);

                    break;
                }
            }
            else
            {
                if (ProceduralUtils.FlagsExist(triggerBlacklist, CardEnums.GetValidFlags<TriggerCondition>(cardDesc.cardType)))
                {
                    cardEffect.triggerCondition = ProceduralUtils.GetRandomValueExcluding(random, model, triggerBlacklist, CardEnums.GetValidFlags<TriggerCondition>(cardDesc.cardType));
                }
                else
                {
                    // No triggers available mean each trigger type has been blacklisted 

                    Debug.Log("No effects are valid for budget " + maxAllowableBudget);

                    break;
                }
            }

            double triggerBudgetModifier = PowerBudget.GetTriggerModifier(cardEffect.triggerCondition, cardDesc);
            maxAllowableBudget /= triggerBudgetModifier;
            minAllowableBudget /= triggerBudgetModifier;

            SortedSet<EffectType> candidatesWithinBudget = ProceduralUtils.GetEffectsWithinBudget(maxAllowableBudget);
            if (candidatesWithinBudget.Count == 0)
            {
                triggerBlacklist.Add(cardEffect.triggerCondition);

                Debug.Log("No effects are valid for budget " + maxAllowableBudget);
                continue;
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

                validEffect = GenerateCardEffect(random, model, creatureModels, cardEffect, effectType, minAllowableBudget, maxAllowableBudget, false);
            }

            // This means that there isn't an effect that meets this condition
            if (validEffect)
            {
                cardDesc.cardEffects.Add(cardEffect);
                break;
            }
            else
            {
                Debug.Log("No valid effect could be generated for trigger <" + cardEffect.triggerCondition.ToString() + "> with budget " + -maxAllowableBudget);
            }
        }
    }

    static private bool GenerateCardEffect(System.Random random, IHistogram model, CreatureModelIndex creatureModels, CardEffectDescription effectDesc, EffectType effect, double minBudget, double maxBudget, bool positive)
    {
        IProceduralEffectGenerator effectGen = ProceduralUtils.GetProceduralGenerator(effect);
        effectGen.SetupParameters(random, model, creatureModels, minBudget, maxBudget);
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
            allowableTargetting.IntersectWith(CardEnums.GetValidFlags<TargettingType>(effectDesc.triggerCondition));

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
