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
        return ((triggerCondition == TriggerCondition.NONE) ? "" : CardParsing.Parse(triggerCondition) + ": ")
            + CardParsing.CapitalizeSentence(targettingType.CardText() + " " + effectType.CardText(targettingType.RequiresPluralEffect()));
    }

    public Alignment GetAlignment()
    {
        return CardEnums.CombineAlignments(targettingType.GetAlignment(), effectType.GetAlignment());
    }

    public double PowerLevel()
    {
        return PowerBudget.FLAT_EFFECT_COST + targettingType.PowerLevel() * effectType.PowerLevel();
    }

    public Queue<EffectResolutionTask> GetEffectTasks(Targettable[] targets, PlayerController player)
    {
        return targettingType.GetEffectTasksWithTargets(effectType, targets, player);
    }
}
