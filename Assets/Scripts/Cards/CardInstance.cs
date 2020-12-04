using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class CardInstance
{
    public CardDescription baseCard;
    public int cardSeed;
    public CardGenerationFlags cardFlags;
    public PlayerController srcPlayer;

    // TODO: Add card modifications

    public CardInstance(PlayerController src, int seed, CardGenerationFlags flags = CardGenerationFlags.NONE)
    {
        srcPlayer = src;
        cardSeed = seed;
        cardFlags = flags;
        baseCard = src.cardGenerator.GenerateCard(seed, flags);
    }

    public CardInstance(CardDescription card)
    {
        baseCard = card;
    }

    public CardInstance Clone()
    {
        CardInstance cardInstance = new CardInstance(srcPlayer, cardSeed, cardFlags);
        return cardInstance;
    }

    public CardType GetCardType()
    {
        return baseCard.cardType;
    }

    public string GetCardName()
    {
        return baseCard.cardName;
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

    public List<CardEffectDescription> GetEffectsOnTrigger(TriggerCondition trigger)
    {
        return baseCard.GetEffectsOnTrigger(trigger);
    }

    public List<CardEffectDescription> GetSelectableEffectsOnTrigger(TriggerCondition trigger)
    {
        return baseCard.GetSelectableEffectsOnTrigger(trigger);
    }

    public bool HasEffectsOnTrigger(TriggerCondition trigger)
    {
        return baseCard.HasEffectsOnTrigger(trigger);
    }
    public List<ITargettingDescription> GetSelectableTargets(TriggerCondition trigger)
    {
        return baseCard.GetSelectableTargets(trigger);
    }

    public SortedSet<KeywordAttribute> GetAttributes()
    {
        if (GetCardType() == CardType.CREATURE)
        {
            return new SortedSet<KeywordAttribute>((baseCard as CreatureCardDescription).attributes);
        }

        return new SortedSet<KeywordAttribute>();
    }

    public Texture2D GetImage()
    {
        return baseCard.image;
    }

    public bool HasKeywordAttribute(KeywordAttribute keyword)
    {
        return baseCard.HasKeywordAttribute(keyword);
    }

}
