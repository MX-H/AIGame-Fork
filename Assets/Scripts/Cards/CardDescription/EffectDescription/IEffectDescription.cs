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

public abstract class IEffectGenerator
{
    public abstract IEffectDescription Generate();
}

public abstract class IProceduralEffectGenerator : IEffectGenerator
{
    protected System.Random random;
    protected CardHistogram model;
    protected double allocatedBudget;

    public void SetupParameters(System.Random r, CardHistogram m, double budget)
    {
        random = r;
        model = m;
        allocatedBudget = budget;
    }
}