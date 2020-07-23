using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NegateEffectDescription : IEffectDescription
{
    public NegateEffectDescription() : base(EffectType.NEGATE)
    { }

    public override string CardText()
    {
        return "is(are) negated";
    }
}
