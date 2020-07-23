using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IContainerEffectDescription : IEffectDescription
{
    IContainerEffectDescription(EffectType e) : base(e)
    { }

    public abstract TargettingType GetResultTargetType();
}
