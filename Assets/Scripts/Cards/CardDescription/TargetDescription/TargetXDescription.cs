using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class TargetXDescription : IQualifiableTargettingDescription
{
    public int amount;

    public TargetXDescription(TargetType target) : base(target, TargettingType.TARGET)
    {
    }

    public override string CardText(bool plural)
    {
        plural = amount != 1;
        string targetString = QualifierText() + CardParsing.Parse(targetType, plural);
        return (plural ? amount.ToString() + " target " : "target ") + targetString;
    }

    public override double PowerLevel()
    {
        return QualifierPowerLevel() * amount;
    }

    public override bool RequiresPluralEffect()
    {
        return amount == 1;
    }

    public override bool RequiresSelection()
    {
        return true;
    }
}

public class TargetXProceduralGenerator : IProceduralTargettingGenerator
{
    private int MinTargets()
    {
        return 1;
    }

    private int MaxTargets()
    {
        switch (targetType)
        {
            case TargetType.CREATURES:
                return 3;
        }
        return 1;
    }
    public override ITargettingDescription Generate()
    {
        TargetXDescription desc = new TargetXDescription(targetType);

        // Find the bounds of card amounts
        int max = ProceduralUtils.GetUpperBound(desc, ref desc.amount, MinTargets(), MaxTargets(), maxAllocatedBudget);
        int min = ProceduralUtils.GetLowerBound(desc, ref desc.amount, MinTargets(), max, minAllocatedBudget);

        Assert.IsTrue(max >= min);
        desc.amount = random.Next(min, max);

        return desc;
    }

    public override ITargettingDescription GetDescriptionType()
    {
        return new TargetXDescription(targetType);
    }

    public override double GetMinCost()
    {
        TargetXDescription desc = new TargetXDescription(targetType);
        desc.amount = 1;
        return desc.PowerLevel();
    }
}