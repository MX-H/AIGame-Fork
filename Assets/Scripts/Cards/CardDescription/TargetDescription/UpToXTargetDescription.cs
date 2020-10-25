using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;


public class UpToXTargetDescription : IQualifiableTargettingDescription
{
    public int amount;

    public UpToXTargetDescription(TargetType target) : base(target, TargettingType.UP_TO_TARGET)
    {
    }

    public override string CardText(bool plural)
    {
        return "up to " + amount.ToString() + " target " + QualifierText() + CardParsing.Parse(targetType, amount != 1);
    }
    public override double PowerLevel()
    {
        return QualifierPowerLevel() * (amount + 0.5);
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

public class UpToXProceduralGenerator : IProceduralTargettingGenerator
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
        UpToXTargetDescription desc = new UpToXTargetDescription(targetType);

        // Find the bounds of card amounts
        int max = ProceduralUtils.GetUpperBound(desc, ref desc.amount, MinTargets(), MaxTargets(), maxAllocatedBudget);
        int min = ProceduralUtils.GetLowerBound(desc, ref desc.amount, MinTargets(), max, minAllocatedBudget);

        Assert.IsTrue(max >= min);
        desc.amount = random.Next(min, max);

        return desc;
    }

    public override ITargettingDescription GetDescriptionType()
    {
        return new UpToXTargetDescription(targetType);
    }

    public override double GetMinCost()
    {
        UpToXTargetDescription desc = new UpToXTargetDescription(targetType);
        desc.amount = 1;
        return desc.PowerLevel();
    }
}