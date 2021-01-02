using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public abstract class IModifierDescription : IDescription
{
    public readonly ModifierType modifierType;
    protected IModifierDescription(ModifierType modifier)
    {
        modifierType = modifier;
    }

    public abstract string CardText(bool plural = false);
    public abstract Alignment GetAlignment();
    public abstract double PowerLevel();
    public abstract IModifier GetModifier(DurationType duration);
    public abstract IModifier GetModifier(Creature source);

}

public abstract class IModifierGenerator
{
    public abstract IModifierDescription Generate();
}

public abstract class IProceduralModifierGenerator : IModifierGenerator
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

    public ModifierType GetModifierType()
    {
        return GetDescriptionType().modifierType;
    }
    public abstract IModifierDescription GetDescriptionType();

    public abstract double GetMinCost();
}