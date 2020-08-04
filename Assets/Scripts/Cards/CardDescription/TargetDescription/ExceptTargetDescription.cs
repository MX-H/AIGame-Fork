using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ExceptTargetDescription : IQualifiableTargettingDescription
{
    public ITargettingDescription targetDescription;

    public ExceptTargetDescription(TargetType target) : base(target, TargettingType.EXCEPT)
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
}

public class ExceptTargetProceduralGenerator : IProceduralTargettingGenerator
{
    public override ITargettingDescription Generate()
    {
        ExceptTargetDescription desc = new ExceptTargetDescription(targetType);
        IProceduralTargettingGenerator targetGen = ProceduralUtils.GetProceduralGenerator(TargettingType.TARGET);
        targetGen.SetupParameters(targetType, random, model, minAllocatedBudget, maxAllocatedBudget);
        desc.targetDescription = targetGen.Generate();
        return desc;
    }

    public override ITargettingDescription GetDescriptionType()
    {
        return new ExceptTargetDescription(targetType);
    }
}