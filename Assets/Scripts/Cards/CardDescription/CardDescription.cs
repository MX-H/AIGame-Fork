using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New CardDescription", menuName = "CardDescription", order = 52)]
public class CardDescription : ScriptableObject, IDescription
{
    public string name;
    public int manaCost;
    public CardType cardType;
    public List<CardEffectDescription> cardEffects = new List<CardEffectDescription>();
    public Texture2D image;

    public string CardText(bool plural)
    {
        return "";
    }

    public Alignment GetAlignment()
    {
        return Alignment.NEUTRAL;
    }

    public double PowerLevel()
    {
        double power = 0;
        foreach (CardEffectDescription effect in cardEffects)
        {
            if (effect.GetAlignment() != Alignment.NEGATIVE)
            {
                power += effect.PowerLevel();
            }
            else {
                power -= effect.PowerLevel() * PowerBudget.DOWNSIDE_WEIGHT;
            }
        }
        return power;
    }

    public List<CardEffectDescription> GetEffectsOnTrigger(TriggerCondition trigger)
    {
        List<CardEffectDescription> effects = new List<CardEffectDescription>();
        foreach (CardEffectDescription effect in cardEffects)
        {
            if (cardType == CardType.CREATURE && trigger != effect.triggerCondition)
            {
                continue;
            }

            effects.Add(effect);
        }
        return effects;
    }

    public List<CardEffectDescription> GetSelectableEffectsOnTrigger(TriggerCondition trigger)
    {
        List<CardEffectDescription> effectList = new List<CardEffectDescription>();

        foreach (CardEffectDescription effect in cardEffects)
        {
            if (effect.targettingType != null && effect.targettingType.RequiresSelection())
            {
                // For creature cards they have effects that trigger at different times
                // make sure the trigger condition matches with the effect

                if (cardType == CardType.CREATURE && trigger != effect.triggerCondition)
                {
                    continue;
                }

                effectList.Add(effect);
            }
        }
        return effectList;
    }

    public bool HasEffectsOnTrigger(TriggerCondition trigger)
    {
        foreach (CardEffectDescription effect in cardEffects)
        {
            if (effect.triggerCondition == trigger)
            {
                return true;
            }
        }
        return false;
    }

    public List<ITargettingDescription> GetSelectableTargets(TriggerCondition trigger)
    {
        List<ITargettingDescription> targetList = new List<ITargettingDescription>();

        foreach (CardEffectDescription effect in cardEffects)
        {
            if (effect.targettingType != null && effect.targettingType.RequiresSelection())
            {
                // For creature cards they have effects that trigger at different times
                // make sure the trigger condition matches with the effect

                if (cardType == CardType.CREATURE && trigger != effect.triggerCondition)
                {
                    continue;
                }

                targetList.Add(effect.targettingType);
            }
        }
        return targetList;
    }
}
