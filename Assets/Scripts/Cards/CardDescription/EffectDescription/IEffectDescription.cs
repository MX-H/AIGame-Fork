﻿using System.Collections;
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

    public abstract string CardText(bool plural);
    public abstract Alignment GetAlignment();
    public abstract double PowerLevel();
    public abstract void ApplyToTarget(Targettable target, PlayerController player, Targettable source);
    public virtual System.Type GetAcknowlegementType()
    {
        return null;
    }
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
    protected CreatureModelIndex creatureModelIndex;

    public void SetupParameters(System.Random r, IHistogram m, CreatureModelIndex creatureModels, double minBudget, double maxBudget)
    {
        random = r;
        model = m;
        creatureModelIndex = creatureModels;
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