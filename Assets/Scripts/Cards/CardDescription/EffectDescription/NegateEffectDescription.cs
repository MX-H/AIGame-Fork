using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NegateEffectDescription : IEffectDescription
{
    public NegateEffectDescription() : base(EffectType.NEGATE)
    { }

    public override void ApplyToTarget(Targettable target, PlayerController player)
    {
        Effect effectTarget = target as Effect;
        if (effectTarget)
        {
            GameUtils.GetGameSession().ServerRemoveEffect(effectTarget);
        }
    }

    public override string CardText(bool plural)
    {
        return (plural ? "is" : "are") + " negated";
    }

    public override Alignment GetAlignment()
    {
        return Alignment.NEGATIVE;
    }

    public override double PowerLevel()
    {
        return PowerBudget.UNIT_COST * 3;
    }

}

public class NegateEffectProceduralGenerator : IProceduralEffectGenerator
{
    public override IEffectDescription Generate()
    {
        return new NegateEffectDescription();
    }

    public override IEffectDescription GetDescriptionType()
    {
        return new NegateEffectDescription();
    }

    public override double GetMinCost()
    {
        return GetDescriptionType().PowerLevel();
    }
}