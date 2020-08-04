using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfTargettingDescription : ITargettingDescription
{
    public SelfTargettingDescription() : base(TargetType.PLAYERS, TargettingType.SELF)
    {
    }
    public override string CardText(bool plural)
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

    public override bool RequiresPluralEffect()
    {
        return false;
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
}