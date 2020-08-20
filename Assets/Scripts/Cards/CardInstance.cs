using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class CardInstance
{
    public CardDescription baseCard;
    public int cardSeed;
    public PlayerController srcPlayer;

    // TODO: Add card modifications

    public CardInstance(PlayerController src, int seed)
    {
        srcPlayer = src;
        cardSeed = seed;
        baseCard = src.cardGenerator.GenerateCard(seed);
    }

    public CardInstance(CardDescription card)
    {
        baseCard = card;
    }

    public CardType GetCardType()
    {
        return baseCard.cardType;
    }

    public string GetCardName()
    {
        return baseCard.name;
    }

    public int GetManaCost()
    {
        return baseCard.manaCost;
    }

    public List<CardEffectDescription> GetCardEffects()
    {
        return baseCard.cardEffects;
    }

    public int GetAttackVal()
    {
        if (GetCardType() == CardType.CREATURE)
        {
            return (baseCard as CreatureCardDescription).attack;
        }

        return 0;
    }
    public int GetHealthVal()
    {
        if (GetCardType() == CardType.CREATURE)
        {
            return (baseCard as CreatureCardDescription).health;
        }

        return 0;
    }
    public CreatureType GetCreatureType()
    {
        if (GetCardType() == CardType.CREATURE)
        {
            return (baseCard as CreatureCardDescription).creatureType;
        }

        return CreatureType.HUMAN;
    }

    public SortedSet<KeywordAttribute> GetAttributes()
    {
        if (GetCardType() == CardType.CREATURE)
        {
            return (baseCard as CreatureCardDescription).attributes;
        }

        return new SortedSet<KeywordAttribute>();
    }

    public Texture2D GetImage()
    {
        return baseCard.image;
    }

}
