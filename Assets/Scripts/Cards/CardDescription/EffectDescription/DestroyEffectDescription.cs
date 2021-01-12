using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class DestroyEffectDescription : IEffectDescription
{
    public DestroyEffectDescription() : base(EffectType.DESTROY_CARD)
    { }

    public override void ApplyToTarget(Targettable target, PlayerController player, Targettable source)
    {
        GameSession gameSession = GameUtils.GetGameSession();
        gameSession.ServerDestroyCard(target);
    }

    public override string CardText(bool plural)
    {
        return plural ? "destroy" : "destroys";
    }
    public override Alignment GetAlignment()
    {
        return Alignment.NEGATIVE;
    }
    public override double PowerLevel()
    {
        return PowerBudget.UNIT_COST * 2;
    }
}

public class DestroyEffectProceduralGenerator : IProceduralEffectGenerator
{
    public override IEffectDescription Generate()
    {
        return new DestroyEffectDescription();
    }

    public override IEffectDescription GetDescriptionType()
    {
        return new DestroyEffectDescription();
    }

    public override double GetMinCost()
    {
        return GetDescriptionType().PowerLevel();
    }
}