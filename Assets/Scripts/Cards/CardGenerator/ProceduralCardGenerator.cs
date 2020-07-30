using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class ProceduralCardGenerator : ICardGenerator
{
    private CardHistogram model;
    public ProceduralCardGenerator(CardHistogram m)
    {
        model = m;
    }

    public override CardDescription GenerateCard(int seed)
    {
        // First select the card type
        System.Random random = new System.Random(seed);
        CardType t = getRandomValue<CardType>(random, model);

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

    static private T getRandomValue<T>(System.Random random, CardHistogram model) where T : Enum
    {
        return getRandomValue<T>(random, model, (T[])Enum.GetValues(typeof(T)));
    }
    static private T getRandomValue<T>(System.Random random, CardHistogram model, IEnumerable<T> whitelist) where T : Enum
    {
        int result = random.Next(model.GetTotal<T>(whitelist));
        foreach (T t in whitelist)
        {
            result -= model.GetValue<T>(t);
            if (result < 0)
            {
                return t;
            }
        }

        // Should never get here
        Debug.Log("Wasn't able to generate a random value for " + typeof(T).ToString() + ", returning first value");
        return (T)Enum.GetValues(typeof(T)).GetValue(0);
    }
    static private T getRandomValueExcluding<T>(System.Random random, CardHistogram model, IEnumerable<T> blacklist) where T : Enum
    {
        return getRandomValueExcluding<T>(random, model, blacklist, (T[])Enum.GetValues(typeof(T)));
    }
    static private T getRandomValueExcluding<T>(System.Random random, CardHistogram model, IEnumerable<T> blacklist, IEnumerable<T> whitelist) where T : Enum
    {
        whitelist = whitelist.Except(blacklist);
        int result = random.Next(model.GetTotal<T>(whitelist));
        foreach (T t in whitelist)
        {
            result -= model.GetValue<T>(t);
            if (result < 0)
            {
                return t;
            }
        }

        // Should never get here
        Debug.Log("Wasn't able to generate a random value for " + typeof(T).ToString() + ", returning first value");
        return (T)Enum.GetValues(typeof(T)).GetValue(0);
    }

    static private CardDescription GenerateCreatureCard(System.Random random, CardHistogram model)
    {
        CreatureCardDescription card = new CreatureCardDescription();
        card.creatureType = getRandomValue<CreatureType>(random, model);
        card.manaCost = (int)getRandomValue<ManaCost>(random, model);
        card.name = "A creature card";

        // Decide on power budget

        float powerBudget = CardEnums.ManaPowerBudgets[card.manaCost];

        // Decide on stats
        card.attack = random.Next(card.manaCost);
        card.health = card.manaCost - card.attack + 1;

        // Decide on keyword attributes
        int amount = random.Next(2);
        for (int i = 0; i < amount; i++)
        {
            card.attributes.Add(getRandomValue<KeywordAttribute>(random, model));
        }

        // Decide on effects
        GenerateCardEffects(random, model, card, powerBudget);

        return card;
    }

    static private CardDescription GenerateSpellCard(System.Random random, CardHistogram model)
    {
        CardDescription card = new CardDescription();
        card.cardType = CardType.SPELL;
        card.manaCost = (int)getRandomValue<ManaCost>(random, model);
        card.name = "A spell card";

        float powerBudget = CardEnums.ManaPowerBudgets[card.manaCost];
        GenerateCardEffects(random, model, card, powerBudget);

        return card;
    }

    static private CardDescription GenerateTrapCard(System.Random random, CardHistogram model)
    {
        CardDescription card = new CardDescription();
        card.cardType = CardType.TRAP;
        card.manaCost = (int)getRandomValue<ManaCost>(random, model);
        card.name = "A trap card";

        float powerBudget = CardEnums.ManaPowerBudgets[card.manaCost];
        GenerateCardEffects(random, model, card, powerBudget);

        return card;
    }

    static private void GenerateCardEffects(System.Random random, CardHistogram model, CardDescription cardDesc, float powerBudget)
    {
        // A trap card only has one trigger condition
        if (cardDesc.cardType == CardType.TRAP)
        {
            getRandomValue(random, model, CardEnums.GetValidFlags<TriggerCondition>(CardType.TRAP));
        }

        int effectCount = 0;
        while (powerBudget > 0 || effectCount >= 5)
        {
            // Generate effects
            CardEffectDescription cardEffect = new CardEffectDescription();

            if (cardDesc.cardType == CardType.SPELL || (cardDesc.cardType == CardType.TRAP && effectCount > 0))
            {
                // After the first condition of a trap or after casting a spell all further effects of
                // the card should just resolve so trigger cond is NONE
                cardEffect.triggerCondition = TriggerCondition.NONE;
            }
            else {
                cardEffect.triggerCondition = getRandomValue(random, model, CardEnums.GetValidFlags<TriggerCondition>(cardDesc.cardType));
            }

            EffectType effectType = getRandomValueExcluding(random, model, new EffectType[] { EffectType.NONE },
                CardEnums.GetValidFlags<EffectType>(cardEffect.triggerCondition));

            // This means that there isn't an effect that meets this condition
            if (effectType == EffectType.NONE)
            {
                continue;
            }
            float powerLevel = GenerateCardEffect(random, model, cardEffect, effectType, powerBudget);
            cardDesc.cardEffects.Add(cardEffect);
            powerBudget -= powerLevel;
            effectCount++;
        }
    }

    static private float GenerateCardEffect(System.Random random, CardHistogram model, CardEffectDescription effectDesc, EffectType effect, float powerBudget)
    {
        float powerLevel = 5.0f; // Every effect adds a base powerlevel

        switch (effect)
        {
            case EffectType.DRAW_CARDS:
                {
                    DrawEffectDescription drawEffect = new DrawEffectDescription();
                    drawEffect.drawModifier = getRandomValue<DrawModifier>(random, model);
                    drawEffect.amount = random.Next(1, 3);
                    effectDesc.effectType = drawEffect;
                }
                break;
            case EffectType.DEAL_DAMAGE:
            case EffectType.HEAL_DAMAGE:
                {
                    DamageEffectDescription damageEffect = new DamageEffectDescription(effect == EffectType.HEAL_DAMAGE);
                    damageEffect.amount = random.Next(1, 3);
                    effectDesc.effectType = damageEffect;
                }
                break;
            case EffectType.SUMMON_TOKEN:
                {
                    SummonEffectDescription summonEffect = new SummonEffectDescription();
                    summonEffect.amount = random.Next(1, 2);
                    summonEffect.tokenType = getRandomValue<CreatureType>(random, model);
                    effectDesc.effectType = summonEffect;
                }
                break;
            case EffectType.NEGATE:
                {
                    NegateEffectDescription negateEffect = new NegateEffectDescription();
                    effectDesc.effectType = negateEffect;
                }
                break;
            default:
                Debug.Log("Didn't set an IEffectDescription for " + effect);
                break;
        }

        effectDesc.targetType = getRandomValue(random, model, CardEnums.GetValidFlags<TargetType>(effect));
        effectDesc.targettingType = getRandomValue(random, model, CardEnums.GetValidFlags<TargettingType>(new object[] { effectDesc.targetType }));

        return powerLevel;
    }
}
