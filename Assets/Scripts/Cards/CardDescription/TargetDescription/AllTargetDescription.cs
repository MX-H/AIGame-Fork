using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AllTargetDescription : IQualifiableTargettingDescription
{
    public AllTargetDescription(TargetType target, Alignment alignment) 
        : base(target, (alignment == Alignment.NEUTRAL) ? TargettingType.ALL : ((alignment == Alignment.POSITIVE) ? TargettingType.ALL_ALLY : TargettingType.ALL_ENEMY), alignment)
    {
    }

    public override string CardText(bool plural)
    {

        if (targetType == TargetType.PLAYERS)
        {
            return "each player";
        }

        return "all " + QualifierText() + CardParsing.Parse(targetType, true);
    }

    public override double PowerLevel()
    {
        return ((GetAlignment() == Alignment.NEUTRAL) ?  1.5 : 3.0) * QualifierPowerLevel();
    }

    public override bool RequiresPluralEffect()
    {
        if (targetType == TargetType.PLAYERS)
        {
            return true;
        }
        return false;
    }

    public override bool RequiresSelection()
    {
        return false;
    }

    public bool AppliesToTargettable(PlayerController player, Targettable target)
    {
        TargetXDescription targetDescription = new TargetXDescription(targetType, GetPlayerAlignment());
        targetDescription.amount = 1;
        targetDescription.qualifier = qualifier;

        TargettingQuery query = new TargettingQuery(targetDescription, player, false);
        return target.IsTargettable(query);
    }

    public override Queue<EffectResolutionTask> GetEffectTasksWithTargets(IEffectDescription effect, Targettable[] targets, PlayerController player, Targettable source)
    {
        Queue<EffectResolutionTask> tasks = new Queue<EffectResolutionTask>();
        TargetXDescription targetDescription = new TargetXDescription(targetType, GetPlayerAlignment());
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
                task.source = source;

                tasks.Enqueue(task);
            }
        }

        return tasks;
    }
}

public class AllTargetProceduralGenerator : IProceduralTargettingGenerator
{
    Alignment alignment;

    public AllTargetProceduralGenerator(Alignment a)
    {
        alignment = a;
    }

    public override ITargettingDescription Generate()
    {
        AllTargetDescription desc = new AllTargetDescription(targetType, alignment);
        return desc;
    }

    public override ITargettingDescription GetDescriptionType()
    {
        return new AllTargetDescription(targetType, alignment);
    }
}