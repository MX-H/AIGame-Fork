using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AllTargetDescription : IQualifiableTargettingDescription
{
    public AllTargetDescription(TargetType target) : base(target, TargettingType.ALL)
    {
    }

    public override string CardText(bool plural)
    {
        return "all " + QualifierText() + CardParsing.Parse(targetType, true);
    }

    public override double PowerLevel()
    {
        return ((GetAlignment() == Alignment.NEUTRAL) ?  1.5 : 3.0) * QualifierPowerLevel();
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
        Queue<EffectResolutionTask> tasks = new Queue<EffectResolutionTask>();
        TargetXDescription targetDescription = new TargetXDescription(targetType);
        targetDescription.amount = 1;
        targetDescription.qualifier = qualifier;

        TargettingQuery query = new TargettingQuery(targetDescription, player, false);
        GameSession gameSession = GameUtils.GetGameSession();

        List<Targettable> possibleTargets = gameSession.GetPotentialTargets();
        foreach (Targettable t in possibleTargets)
        {
            if (t.IsTargettable(query))
            {
                EffectResolutionTask task = new EffectResolutionTask();
                task.effect = effect;
                task.target = t.GetTargettableEntity();
                task.player = player;

                tasks.Enqueue(task);
            }
        }

        return tasks;
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
}