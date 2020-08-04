using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Assertions;

public abstract class IEffectDescription : IDescription
{
    public readonly EffectType effectType;
    public readonly TriggerCondition triggerCondition;
    protected IEffectDescription(EffectType effect)
    {
        effectType = effect;
    }

    public abstract string CardText();
    public abstract Alignment GetAlignment();
    public abstract double PowerLevel();
}

public abstract class IEffectGenerator
{
    public abstract IEffectDescription Generate();
}

public abstract class IProceduralEffectGenerator : IEffectGenerator 
{
    protected System.Random random;
    protected IHistogram model;
    protected double minAllocatedBudget;
    protected double maxAllocatedBudget;

    public void SetupParameters(System.Random r, IHistogram m, double minBudget, double maxBudget)
    {
        random = r;
        model = m;
        Assert.IsTrue(maxBudget >= minBudget);
        minAllocatedBudget = minBudget;
        maxAllocatedBudget = maxBudget;
    }

    public EffectType GetEffectType()
    {
        return GetDescriptionType().effectType;
    }
    public abstract IEffectDescription GetDescriptionType();

    public abstract double GetMinCost();
}