using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

static public class CardGenerator
{
    static private T getRandomValue<T>(System.Random random, CardHistogram model) where T : Enum
    {
        return getRandomValue<T>(random, model, (T[]) Enum.GetValues(typeof(T)));
    }
    static private T getRandomValue<T>(System.Random random, CardHistogram model, T[] whitelist) where T : Enum
    {
        int result = random.Next(model.GetTotal<T>());
        int size = model.GetTotal<T>();
        int startingVal = result;
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

    static private CardDescription generateCreatureCard(System.Random random, CardHistogram model)
    {
        CreatureCardDescription card = new CreatureCardDescription();
        card.creatureType = getRandomValue<CreatureType>(random, model);
        card.manaCost = (int)getRandomValue<ManaCost>(random, model);
        card.name = "A creature card";

        return card;
    }

    static private CardDescription generateSpellCard(System.Random random, CardHistogram model)
    {
        CardDescription card = new CardDescription();
        card.manaCost = (int)getRandomValue<ManaCost>(random, model);
        card.name = "A spell card";

        return card;
    }

    static private CardDescription generateTrapCard(System.Random random, CardHistogram model)
    {
        CardDescription card = new CardDescription();
        card.manaCost = (int)getRandomValue<ManaCost>(random, model);
        card.name = "A trap card";

        return card;
    }

    static public CardDescription generateCard(int seed, CardHistogram model)
    {
        // First select the card type
        System.Random random = new System.Random(seed);
        CardType t = getRandomValue<CardType>(random, model);

        switch (t)
        {
            case CardType.CREATURE:
                return generateCreatureCard(random, model);
            case CardType.SPELL:
                return generateSpellCard(random, model);
            case CardType.TRAP:
                return generateTrapCard(random, model);
        }

        return new CardDescription();
    }
}
