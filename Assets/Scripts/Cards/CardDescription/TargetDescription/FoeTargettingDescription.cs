using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoeTargettingDescription : ITargettingDescription
{
    public FoeTargettingDescription() : base(TargetType.PLAYERS, TargettingType.FOE)
    {
    }
    public override string CardText(bool plural)
    {
        return "opponent";
    }

    public override Alignment GetAlignment()
    {
        return Alignment.NEGATIVE;
    }

    public override double PowerLevel()
    {
        return 1.0;
    }

    public override bool RequiresPluralEffect()
    {
        return true;
    }

    public override bool RequiresSelection()
    {
        return false;
    }

    public override void ResolveEffectWithTargets(IEffectDescription effect, Targettable[] targets, PlayerController player)
    {
        effect.ApplyToTarget(player.GetOpponents()[0], player);
    }
}

public class FoeTargettingProceduralGenerator : IProceduralTargettingGenerator
{
    public override ITargettingDescription Generate()
    {
        return new FoeTargettingDescription();
    }

    public override ITargettingDescription GetDescriptionType()
    {
        return new FoeTargettingDescription();
    }
}