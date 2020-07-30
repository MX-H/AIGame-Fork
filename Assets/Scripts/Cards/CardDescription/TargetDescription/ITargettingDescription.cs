using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ITargettingDescription
{
    public readonly TargetType targetType;
    public readonly TargettingType targettingType;

    protected ITargettingDescription(TargetType target, TargettingType targetting)
    {
        targetType = target;
        targettingType = targetting;
    }
    public abstract Classification GetClassification();

}
