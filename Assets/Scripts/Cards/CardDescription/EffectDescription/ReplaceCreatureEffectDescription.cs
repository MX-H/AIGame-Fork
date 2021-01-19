using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// THIS IS NOT A REAL EFFECT THAT GETS GENERATED
// THIS IS ONLY USED FOR REPLACING A CREATURE WHEN PLAYING
// A CREATURE FROM HAND WITH A FULL BOARD

public class ReplaceCreatureEffectDescription : IEffectDescription
{
    public ReplaceCreatureEffectDescription() : base(EffectType.NONE)
    { }

    public override void ApplyToTarget(Targettable target, PlayerController player, Targettable source)
    {

    }

    public override string CardText(bool plural)
    {
        return "replace";
    }

    public override Alignment GetAlignment()
    {
        return Alignment.NEUTRAL;
    }

    public override double PowerLevel()
    {
        return  0;
    }

}
