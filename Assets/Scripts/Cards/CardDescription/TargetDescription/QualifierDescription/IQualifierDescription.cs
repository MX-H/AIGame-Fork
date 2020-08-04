using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public abstract class IQualifierDescription : IDescription
{
    public readonly QualifierType qualifierType;

    protected IQualifierDescription(QualifierType qualifier)
    {
        qualifierType = qualifier;
    }

    public abstract string CardText(bool plural = false);
    public abstract Alignment GetAlignment();
    public abstract double PowerLevel();
}

public abstract class IQualifierGenerator
{
    public abstract IQualifierDescription Generate();
}

public abstract class IProceduralQualifierGenerator : IQualifierGenerator
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

    public QualifierType GetQualifierType()
    {
        return GetDescriptionType().qualifierType;
    }
    public abstract IQualifierDescription GetDescriptionType();

    public abstract double GetMinCost();
}