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
        return "the opponent";
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

    public override Queue<EffectResolutionTask> GetEffectTasksWithTargets(IEffectDescription effect, Targettable[] targets, PlayerController player, Targettable source)
    {
        EffectResolutionTask task = new EffectResolutionTask();
        task.effect = effect;
        task.target = player.GetOpponents()[0];
        task.player = player;
        task.source = source;

        return new Queue<EffectResolutionTask>( new EffectResolutionTask[] { task });
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