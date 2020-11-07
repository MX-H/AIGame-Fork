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

    public override bool RequiresSelection()
    {
        return targetDescription.RequiresSelection();
    }

    public override void ResolveEffectWithTargets(IEffectDescription effect, Targettable[] targets, PlayerController player)
    {
        TargetXDescription targetDescription = new TargetXDescription(targetType);
        targetDescription.amount = 1;
        targetDescription.qualifier = qualifier;

        TargettingQuery query = new TargettingQuery(targetDescription, player);
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
                    effect.ApplyToTarget(targetableEntity, player);
                }
            }
        }
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