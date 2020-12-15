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

    public override bool RequiresSelection()
    {
        return false;
    }

    public override Queue<EffectResolutionTask> GetEffectTasksWithTargets(IEffectDescription effect, Targettable[] targets, PlayerController player)
    {
        EffectResolutionTask task = new EffectResolutionTask();
        task.effect = effect;
        task.target = player;
        task.player = player;

        return new Queue<EffectResolutionTask>(new EffectResolutionTask[] { task });
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