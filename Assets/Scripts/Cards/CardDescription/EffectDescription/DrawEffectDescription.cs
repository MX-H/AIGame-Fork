using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawEffectDescription : IEffectDescription
{
    public DrawModifier drawModifier;
    public int amount;
    public DrawEffectDescription() : base(EffectType.DRAW_CARDS)
    {
        drawModifier = DrawModifier.SELF;
    }

    public override string CardText() 
    {
        return "draw(s) " + amount.ToString() + " card(s)";
    }
}
