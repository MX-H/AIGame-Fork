using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManaCostModifierDescription : IModifierDescription
{
    public int manaCost;
    public bool positive; // Positive is true for mana cost reduction
    public ManaCostModifierDescription(bool isPositive) : base(ModifierType.MANA_COST)
    {
        positive = isPositive;
        manaCost = 0;
    }

    public override string CardText(bool plural)
    {
        string prefix = positive ? "-" : "+";
        return prefix + manaCost.ToString() + " Mana Cost";
    }

    public override Alignment GetAlignment()
    {
        return positive ? Alignment.POSITIVE : Alignment.NEGATIVE;
    }

    public override IModifier GetModifier(DurationType duration)
    {
        return positive ? new ManaCostModifier(-manaCost, duration) : new ManaCostModifier(manaCost, duration);
    }

    public override IModifier GetModifier(Creature source)
    {
        return positive ? new ManaCostModifier(-manaCost, source) : new ManaCostModifier(manaCost, source);
    }

    public override double PowerLevel()
    {
        return ((manaCost * 1.5) + 1) * PowerBudget.UNIT_COST;
    }
}

public class ManaCostModifierProceduralGenerator : IProceduralModifierGenerator
{
    bool positive;
    private static int MIN_STATS = 1;
    private static int MAX_STATS = 3;

    public ManaCostModifierProceduralGenerator(bool positiveModifier)
    {
        positive = positiveModifier;
    }

    public override IModifierDescription Generate()
    {
        ManaCostModifierDescription desc = new ManaCostModifierDescription(positive);

        int max = ProceduralUtils.GetUpperBound(desc, ref desc.manaCost, MIN_STATS, MAX_STATS, maxAllocatedBudget);
        int min = ProceduralUtils.GetLowerBound(desc, ref desc.manaCost, MIN_STATS, MAX_STATS, minAllocatedBudget);

        desc.manaCost = random.Next(min, max + 1);

        return desc;
    }

    public override IModifierDescription GetDescriptionType()
    {
        return new ManaCostModifierDescription(positive);
    }

    public override double GetMinCost()
    {
        ManaCostModifierDescription desc = GetDescriptionType() as ManaCostModifierDescription;
        desc.manaCost = 1;

        return desc.PowerLevel();
    }
}