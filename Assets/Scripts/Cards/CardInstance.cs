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

    public List<IModifier> modifiers;

    // TODO: Add card modifications
    public void AddModifier(IModifier modifier)
    {
        modifiers.Add(modifier);
    }

    public void ConvertHandModifiersToCreatureModifiers()
    {
        modifiers.RemoveAll(x => x is ManaCostModifier);

        foreach (IModifier modifier in modifiers)
        {
            modifier.modifierDuration = DurationType.FOREVER;
            modifier.auraSource = null;
        }
    }

    public CardInstance(PlayerController src, int seed, CardGenerationFlags flags = CardGenerationFlags.NONE)
    {
        srcPlayer = src;
        cardSeed = seed;
        cardFlags = flags;
        baseCard = src.cardGenerator.GenerateCard(seed, flags);
        modifiers = new List<IModifier>();
    }

    public CardInstance(CardDescription card)
    {
        baseCard = card;
        modifiers = new List<IModifier>();
    }

    public CardInstance Clone()
    {
        CardInstance cardInstance = new CardInstance(srcPlayer, cardSeed, cardFlags);

        foreach (IModifier modifier in modifiers)
        {
            cardInstance.AddModifier(modifier);
        }

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

    public int GetBaseManaCost()
    {
        return baseCard.manaCost;
    }

    public int GetManaCost()
    {
        int manaCost = baseCard.manaCost;
        foreach (IModifier mod in modifiers)
        {
            if (mod is ManaCostModifier manaMod)
            {
                manaCost += manaMod.manaCostChange;
            }
        }
        return manaCost;
    }

    public List<CardEffectDescription> GetCardEffects()
    {
        return baseCard.cardEffects;
    }

    public void RemoveEndOfTurnModifiers()
    {
        modifiers.RemoveAll(x => x.modifierDuration == DurationType.END_OF_TURN);
    }

    public void RemoveAuraModifiers(Creature source)
    {
        modifiers.RemoveAll(x => x.auraSource == source.netIdentity);
    }

    public void RemoveAllModifiers()
    {
        modifiers.Clear();
    }

    public int GetAttackVal()
    {
        if (GetCardType() == CardType.CREATURE)
        {
            int atk = (baseCard as CreatureCardDescription).attack;
            foreach (IModifier mod in modifiers)
            {
                if (mod is StatModifier statMod)
                {
                    atk += statMod.atkModifier;
                }
            }
            return atk;
        }

        return 0;
    }
    public int GetHealthVal()
    {
        if (GetCardType() == CardType.CREATURE)
        {
            int hp = (baseCard as CreatureCardDescription).health;
            foreach (IModifier mod in modifiers)
            {
                if (mod is StatModifier statMod)
                {
                    hp += statMod.defModifier;
                }
            }
            return hp;
        }

        return 0;
    }

    public int GetBaseAttackVal()
    {
        if (GetCardType() == CardType.CREATURE)
        {
            return (baseCard as CreatureCardDescription).attack;
        }

        return 0;
    }

    public int GetBaseHealthVal()
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
            SortedSet<KeywordAttribute> keywords = new SortedSet<KeywordAttribute>((baseCard as CreatureCardDescription).GetAttributes());
            foreach (IModifier mod in modifiers)
            {
                if (mod is KeywordModifier keywordMod)
                {
                    keywords.Add(keywordMod.keywordAttribute);
                }
            }
            return keywords;
        }

        return new SortedSet<KeywordAttribute>();
    }

    public SortedSet<KeywordAttribute> GetBaseAttributes()
    {
        if (GetCardType() == CardType.CREATURE)
        {
            return new SortedSet<KeywordAttribute>((baseCard as CreatureCardDescription).GetAttributes());
        }

        return new SortedSet<KeywordAttribute>();
    }

    public Texture2D GetImage()
    {
        return baseCard.image;
    }

    public bool HasKeywordAttribute(KeywordAttribute keyword)
    {
        bool hasKeyword = baseCard.HasKeywordAttribute(keyword);

        foreach (IModifier mod in modifiers)
        {
            if (mod is KeywordModifier keywordMod)
            {
                if (keyword == keywordMod.keywordAttribute)
                {
                    hasKeyword = true;
                }
            }
        }

        return hasKeyword;
    }

}
