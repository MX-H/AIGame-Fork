using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChainEffectDescription : IEffectDescription
{
    IEffectDescription effect1;
    IEffectDescription effect2;

    public ChainEffectDescription(): base(EffectType.NONE)
    {
    }

    public override string CardText()
    {
        throw new System.NotImplementedException();
    }

    public override Alignment GetAlignment()
    {
        throw new System.NotImplementedException();
    }

    public override double PowerLevel()
    {
        throw new System.NotImplementedException();
    }
}
