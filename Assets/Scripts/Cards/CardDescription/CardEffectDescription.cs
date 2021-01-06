using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardEffectDescription : IDescription
{
    public TriggerCondition triggerCondition;

    public ITargettingDescription targettingType;
    public IEffectDescription effectType;

    public string CardText(bool plural = false)
    {
        string text = ((triggerCondition == TriggerCondition.NONE) ? "" : CardParsing.Parse(triggerCondition) + ": ");
        if (effectType is DrawEffectDescription || effectType is SummonEffectDescription || effectType is AuraModifierEffectDescription)
        {
            if (targettingType is SelfTargettingDescription)
            {
                text += CardParsing.CapitalizeSentence(effectType.CardText(targettingType.RequiresPluralEffect()));
            }
            else
            {
                text += CardParsing.CapitalizeSentence(targettingType.CardText() + " " + effectType.CardText(targettingType.RequiresPluralEffect()));
            }
        }
        else
        {
            text += CardParsing.CapitalizeSentence(effectType.CardText(false) + " " + targettingType.CardText());
        }
        text += ".";
        return text;
    }

    public Alignment GetAlignment()
    {
        return CardEnums.CombineAlignments(targettingType.GetAlignment(), effectType.GetAlignment());
    }

    public double PowerLevel()
    {
        return PowerBudget.FLAT_EFFECT_COST + targettingType.PowerLevel() * effectType.PowerLevel();
    }

    public Queue<EffectResolutionTask> GetEffectTasks(Targettable[] targets, PlayerController player, Targettable source)
    {
        return targettingType.GetEffectTasksWithTargets(effectType, targets, player, source);
    }
}
