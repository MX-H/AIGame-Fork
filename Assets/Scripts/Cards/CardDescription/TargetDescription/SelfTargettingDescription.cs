using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfTargettingDescription : ITargettingDescription
{
    public SelfTargettingDescription() : base(TargetType.PLAYERS, TargettingType.SELF)
    {
    }
    public override string CardText()
    {
        return "you";
    }

    public override Alignment GetAlignment()
    {
        return Alignment.POSITIVE;
    }

    public override double PowerLevel()
    {
        return 1.0;
    }
}

public class SelfTargettingProceduralGenerator : IProceduralTargettingGenerator
{
    public override ITargettingDescription Generate()
    {
        return new SelfTargettingDescription();
    }

    public override ITargettingDescription GetDescriptionType()
    {
        return new SelfTargettingDescription();
    }

    public override double GetMinCost()
    {
        return 0.0;
    }
}