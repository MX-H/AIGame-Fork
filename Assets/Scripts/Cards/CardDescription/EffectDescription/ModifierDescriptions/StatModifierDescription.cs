using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatModifierDescription : IModifierDescription
{
    public int atk;
    public int def;
    public bool positive;
    public StatModifierDescription(bool isPositive) : base(ModifierType.STAT)
    {
        positive = isPositive;
        atk = 0;
        def = 0;
    }

    public override string CardText(bool plural)
    {
        string prefix = positive ? "+" : "-";
        return prefix + atk.ToString() + "|" + prefix + def.ToString();
    }

    public override Alignment GetAlignment()
    {
        return positive ? Alignment.POSITIVE : Alignment.NEGATIVE;
    }

    public override IModifier GetModifier(DurationType duration)
    {
        return positive ? new StatModifier(atk, def, duration) : new StatModifier(-atk, -def, duration);
    }

    public override IModifier GetModifier(Creature source)
    {
        return positive ? new StatModifier(atk, def, source) : new StatModifier(-atk, -def, source);
    }

    public override double PowerLevel()
    {
        return ((atk + def) / 2.0) * PowerBudget.UNIT_COST;
    }
}

public class StatModifierProceduralGenerator : IProceduralModifierGenerator
{
    bool positive;
    private static int MIN_STATS = 1;
    private static int MAX_STATS = 5;

    public StatModifierProceduralGenerator(bool positiveModifier)
    {
        positive = positiveModifier;
    }

    public override IModifierDescription Generate()
    {
        StatModifierDescription desc = new StatModifierDescription(positive);

        int singleMax = ProceduralUtils.GetUpperBound(desc, ref desc.atk, MIN_STATS, MAX_STATS, maxAllocatedBudget);
        int singleMin = ProceduralUtils.GetLowerBound(desc, ref desc.atk, MIN_STATS, MAX_STATS, minAllocatedBudget);

        int doubleMax = ProceduralUtils.GetUpperBound(desc, ref desc.atk, ref desc.def, MIN_STATS, MAX_STATS, maxAllocatedBudget);
        int doubleMin = ProceduralUtils.GetLowerBound(desc, ref desc.atk, ref desc.def, MIN_STATS, MAX_STATS, minAllocatedBudget);

        bool single = random.NextDouble() > 0.5;
        bool atk = random.NextDouble() > 0.5;

        if (!single && singleMax == 1)
        {
            single = true;
        }
        else if (single && doubleMin * 2 > singleMax)
        {
            single = false;
        }

        if (single)
        {
            int value = random.Next(singleMin, singleMax + 1);

            if (atk)
            {
                desc.atk = value;
            }
            else
            {
                desc.def = value;
            }
        }
        else
        {
            int value = random.Next(doubleMin, doubleMax + 1);
            desc.atk = value;
            desc.def = value;
        }

        return desc;
    }

    public override IModifierDescription GetDescriptionType()
    {
        return new StatModifierDescription(positive);
    }

    public override double GetMinCost()
    {
        StatModifierDescription desc = GetDescriptionType() as StatModifierDescription;
        desc.atk = 1;

        return desc.PowerLevel();
    }
}