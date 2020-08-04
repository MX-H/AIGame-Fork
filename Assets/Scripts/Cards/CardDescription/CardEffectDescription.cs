using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardEffectDescription : IDescription
{
    public TriggerCondition triggerCondition;

    public ITargettingDescription targettingType;
    public IEffectDescription effectType;

    public string CardText()
    {
        return ((triggerCondition == TriggerCondition.NONE) ? "" : CardParsing.Parse(triggerCondition) + ": ")
            + targettingType.CardText() + " " + effectType.CardText();
    }

    public Alignment GetAlignment()
    {
        return CardEnums.CombineAlignments(targettingType.GetAlignment(), effectType.GetAlignment());
    }

    public double PowerLevel()
    {
        return PowerBudget.FLAT_EFFECT_COST + targettingType.PowerLevel() * effectType.PowerLevel();
    }
}
