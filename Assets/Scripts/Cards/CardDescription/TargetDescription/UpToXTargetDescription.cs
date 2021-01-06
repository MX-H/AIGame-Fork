using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;


public class UpToXTargetDescription : IQualifiableTargettingDescription
{
    public int amount;

    public UpToXTargetDescription(TargetType target, Alignment alignment)
        : base(target, (alignment == Alignment.NEUTRAL) ? TargettingType.UP_TO_TARGET : ((alignment == Alignment.POSITIVE) ? TargettingType.UP_TO_TARGET_ALLY : TargettingType.UP_TO_TARGET_ENEMY), alignment)
    {
    }

    public override string CardText(bool plural)
    {
        return "up to " + amount.ToString() + " " + QualifierText() + CardParsing.Parse(targetType, amount != 1);
    }
    public override double PowerLevel()
    {
        return ((GetPlayerAlignment() == Alignment.NEUTRAL) ? 1.25 : 1) * QualifierPowerLevel() * (amount + 0.5);
    }
    public override bool RequiresPluralEffect()
    {
        return amount == 1;
    }

    public override bool RequiresSelection()
    {
        return true;
    }

    public override Queue<EffectResolutionTask> GetEffectTasksWithTargets(IEffectDescription effect, Targettable[] targets, PlayerController player, Targettable source)
    {
        Queue<EffectResolutionTask> tasks = new Queue<EffectResolutionTask>();

        foreach (Targettable target in targets)
        {
            EffectResolutionTask task = new EffectResolutionTask();
            task.effect = effect;
            task.target = target.GetTargettableEntity();
            task.player = player;
            task.source = source;

            tasks.Enqueue(task);
        }

        return tasks;
    }
}

public class UpToXProceduralGenerator : IProceduralTargettingGenerator
{
    Alignment alignment;

    public UpToXProceduralGenerator(Alignment playerAlignment)
    {
        alignment = playerAlignment;
    }

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
        UpToXTargetDescription desc = new UpToXTargetDescription(targetType, alignment);

        // Find the bounds of card amounts
        int max = ProceduralUtils.GetUpperBound(desc, ref desc.amount, MinTargets(), MaxTargets(), maxAllocatedBudget);
        int min = ProceduralUtils.GetLowerBound(desc, ref desc.amount, MinTargets(), max, minAllocatedBudget);

        Assert.IsTrue(max >= min);
        desc.amount = random.Next(min, max);

        return desc;
    }

    public override ITargettingDescription GetDescriptionType()
    {
        return new UpToXTargetDescription(targetType, alignment);
    }

    public override double GetMinCost()
    {
        UpToXTargetDescription desc = new UpToXTargetDescription(targetType, alignment);
        desc.amount = 1;
        return desc.PowerLevel();
    }
}