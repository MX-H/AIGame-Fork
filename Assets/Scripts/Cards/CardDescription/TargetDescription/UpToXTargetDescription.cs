using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpToXTargetDescription : ITargettingDescription
{
    public int amount;
    IQualifierDescription qualifier;

    public UpToXTargetDescription(TargetType target) : base(target, TargettingType.UP_TO_TARGET)
    {
    }

    public override Classification GetClassification()
    {
        return (qualifier == null) ? Classification.NEUTRAL : qualifier.GetClassification();
    }
}
