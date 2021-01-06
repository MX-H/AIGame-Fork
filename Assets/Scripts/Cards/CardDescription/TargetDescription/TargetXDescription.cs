using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class TargetXDescription : IQualifiableTargettingDescription
{
    public int amount;

    public TargetXDescription(TargetType target, Alignment alignment)
        : base(target, (alignment == Alignment.NEUTRAL) ? TargettingType.TARGET : ((alignment == Alignment.POSITIVE) ? TargettingType.TARGET_ALLY : TargettingType.TARGET_ENEMY), alignment)
    {
    }

    public override string CardText(bool plural)
    {
        plural = amount != 1;
        string targetString = QualifierText() + CardParsing.Parse(targetType, plural);
        targetString = (plural ? amount.ToString() : (("aeiouAEIOU".IndexOf(targetString[0]) >= 0) ? "an" : "a")) + " " + targetString;
        return targetString;
        //return (plural ? amount.ToString() + " " : "target ") + targetString;
    }

    public override double PowerLevel()
    {
        return ((GetPlayerAlignment() == Alignment.NEUTRAL) ? 1.25 : 1) * QualifierPowerLevel() * amount;
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

public class TargetXProceduralGenerator : IProceduralTargettingGenerator
{
    Alignment alignment;

    public TargetXProceduralGenerator(Alignment playerAlignment)
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
        TargetXDescription desc = new TargetXDescription(targetType, alignment);

        // Find the bounds of card amounts
        int max = ProceduralUtils.GetUpperBound(desc, ref desc.amount, MinTargets(), MaxTargets(), maxAllocatedBudget);
        int min = ProceduralUtils.GetLowerBound(desc, ref desc.amount, MinTargets(), max, minAllocatedBudget);

        Assert.IsTrue(max >= min);
        desc.amount = random.Next(min, max);

        return desc;
    }

    public override ITargettingDescription GetDescriptionType()
    {
        return new TargetXDescription(targetType, alignment);
    }

    public override double GetMinCost()
    {
        TargetXDescription desc = new TargetXDescription(targetType, alignment);
        desc.amount = 1;
        return desc.PowerLevel();
    }
}