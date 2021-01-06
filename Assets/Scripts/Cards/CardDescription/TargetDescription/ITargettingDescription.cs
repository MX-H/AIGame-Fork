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
    public abstract bool RequiresSelection();
    public abstract bool RequiresPluralEffect();
    public abstract string CardText(bool plural = false);
    public abstract Alignment GetAlignment();
    public abstract double PowerLevel();
    public abstract Queue<EffectResolutionTask> GetEffectTasksWithTargets(IEffectDescription effect, Targettable[] targets, PlayerController player, Targettable source);
}

public struct EffectResolutionTask
{
    public Targettable target;
    public IEffectDescription effect;
    public PlayerController player;
    public Targettable source;
}

public abstract class IQualifiableTargettingDescription : ITargettingDescription
{
    Alignment playerAlignment;

    protected IQualifiableTargettingDescription(TargetType target, TargettingType targetting, Alignment alignment) : base(target, targetting)
    {
        playerAlignment = alignment;
    }

    protected string QualifierText()
    {
        string alignmentText = "";
        switch (GetPlayerAlignment())
        {
            case Alignment.POSITIVE:
                alignmentText = "allied ";
                break;
            case Alignment.NEGATIVE:
                alignmentText = "opposing ";
                break;
        }

        return alignmentText + ((qualifier == null) ? "" : qualifier.CardText() + " ");
    }

    protected double QualifierPowerLevel()
    {
        return (qualifier == null) ? 1.0 : qualifier.PowerLevel();
    }

    protected Alignment QualifierAlignment()
    {
        return (qualifier == null) ? Alignment.NEUTRAL : qualifier.GetAlignment();
    }

    public override Alignment GetAlignment()
    {
        return (GetPlayerAlignment() == Alignment.NEUTRAL) ? QualifierAlignment() : GetPlayerAlignment();
    }

    public Alignment GetPlayerAlignment() // For targetting allied vs enemy things
    {
        return playerAlignment;
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