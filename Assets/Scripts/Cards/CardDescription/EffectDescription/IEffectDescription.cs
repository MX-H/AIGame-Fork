using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IEffectDescription
{
    public readonly EffectType effectType;
    public readonly TriggerCondition triggerCondition;
    protected IEffectDescription(EffectType effect)
    {
        effectType = effect;
    }

    public abstract string CardText();
}
