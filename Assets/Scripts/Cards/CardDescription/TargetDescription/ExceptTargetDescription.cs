using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ExceptTargetDescription : IQualifiableTargettingDescription
{
    public ITargettingDescription targetDescription;

    public ExceptTargetDescription(TargetType target, Alignment alignment)
        : base(target, (alignment == Alignment.NEUTRAL) ? TargettingType.EXCEPT : ((alignment == Alignment.POSITIVE) ? TargettingType.EXCEPT_ALLY : TargettingType.EXCEPT_ENEMY), alignment)
    {
    }

    public override string CardText(bool plural)
    {
        return "all " + QualifierText() + CardParsing.Parse(targetType, true) + " except " + targetDescription.CardText();
    }

    public override double PowerLevel()
    {
        return ((GetAlignment() == Alignment.NEUTRAL) ? 2.0 : 3.5) * QualifierPowerLevel();
    }

    public override bool RequiresPluralEffect()
    {
        return false;
    }

    public override bool RequiresSelection()
    {
        return targetDescription.RequiresSelection();
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
                Targettable targetableEntity = t.GetTargettableEntity();
                bool valid = true;
                foreach (Targettable t2 in targets)
                {
                    if (targetableEntity == t2)
                    {
                        valid = false;
                        break;
                    }
                }
                if (valid)
                {
                    EffectResolutionTask task = new EffectResolutionTask();
                    task.effect = effect;
                    task.target = targetableEntity;
                    task.player = player;
                    task.source = source;

                    tasks.Enqueue(task);
                }
            }
        }
        return tasks;
    }
}

public class ExceptTargetProceduralGenerator : IProceduralTargettingGenerator
{
    Alignment alignment;

    public ExceptTargetProceduralGenerator(Alignment a)
    {
        alignment = a;
    }


    public override ITargettingDescription Generate()
    {
        ExceptTargetDescription desc = new ExceptTargetDescription(targetType, alignment);

        TargettingType exceptTargetting = TargettingType.TARGET;
        switch (desc.GetPlayerAlignment())
        {
            case Alignment.POSITIVE:
                exceptTargetting = TargettingType.TARGET_ALLY;
                break;
            case Alignment.NEGATIVE:
                exceptTargetting = TargettingType.TARGET_ENEMY;
                break;
        }

        IProceduralTargettingGenerator targetGen = ProceduralUtils.GetProceduralGenerator(exceptTargetting);
        targetGen.SetupParameters(targetType, random, model, minAllocatedBudget, maxAllocatedBudget);
        desc.targetDescription = targetGen.Generate();
        if (desc.targetDescription is IQualifiableTargettingDescription qualifiableDesc)
        {
            qualifiableDesc.qualifier = desc.qualifier;
        }
        return desc;
    }

    public override ITargettingDescription GetDescriptionType()
    {
        return new ExceptTargetDescription(targetType, alignment);
    }
}