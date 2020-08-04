using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AllTargetDescription : IQualifiableTargettingDescription
{
    public AllTargetDescription(TargetType target) : base(target, TargettingType.ALL)
    {
    }

    public override string CardText()
    {
        return "all " + QualifierText() + CardParsing.Parse(targetType, true);
    }

    public override double PowerLevel()
    {
        return ((GetAlignment() == Alignment.NEUTRAL) ?  2.0 : 4.0) * QualifierPowerLevel();
    }
}

public class AllTargetProceduralGenerator : IProceduralTargettingGenerator
{
    public override ITargettingDescription Generate()
    {
        AllTargetDescription desc = new AllTargetDescription(targetType);
        return desc;
    }

    public override ITargettingDescription GetDescriptionType()
    {
        return new AllTargetDescription(targetType);
    }

    public override double GetMinCost()
    {
        return GetDescriptionType().PowerLevel();
    }
}