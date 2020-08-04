using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public abstract class ITargettingDescription : IDescription
{
    public readonly TargetType targetType;
    public readonly TargettingType targettingType;

    protected ITargettingDescription(TargetType target, TargettingType targetting)
    {
        targetType = target;
        targettingType = targetting;
    }

    public abstract bool RequiresPluralEffect();

    public abstract string CardText(bool plural = false);
    public abstract Alignment GetAlignment();
    public abstract double PowerLevel();
}

public abstract class IQualifiableTargettingDescription : ITargettingDescription
{
    protected IQualifiableTargettingDescription(TargetType target, TargettingType targetting) : base(target, targetting)
    { }

    protected string QualifierText()
    {
        return (qualifier == null) ? "" : qualifier.CardText() + " ";
    }

    protected double QualifierPowerLevel()
    {
        return (qualifier == null) ? 1.0 : qualifier.PowerLevel();
    }

    protected Alignment QualifierAlignment()
    {
        return (qualifier == null) ? Alignment.NEUTRAL : qualifier.GetAlignment();
    }

    public sealed override Alignment GetAlignment()
    {
        return QualifierAlignment();
    }

    public IQualifierDescription qualifier;
}

public abstract class ITargettingGenerator
{
    public abstract ITargettingDescription Generate();
}

public abstract class IProceduralTargettingGenerator : ITargettingGenerator
{
    protected System.Random random;
    protected IHistogram model;
    protected double minAllocatedBudget;
    protected double maxAllocatedBudget;
    protected TargetType targetType;

    public void SetupParameters(TargetType type, System.Random r, IHistogram m, double minBudget, double maxBudget)
    {
        random = r;
        model = m;
        Assert.IsTrue(maxBudget >= minBudget);
        minAllocatedBudget = minBudget;
        maxAllocatedBudget = maxBudget;
        targetType = type;
    }

    public TargettingType GetTargettingType()
    {
        return GetDescriptionType().targettingType;
    }

    // Min budget used to cull
    public virtual double GetMinCost()
    {
        return GetDescriptionType().PowerLevel();
    }
    public abstract ITargettingDescription GetDescriptionType();
}