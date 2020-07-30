using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetXDescription : ITargettingDescription
{
    public int amount;
    IQualifierDescription qualifier;

    public TargetXDescription(TargetType target) : base(target, TargettingType.TARGET)
    {
    }

    public override Classification GetClassification()
    {
        return (qualifier == null) ? Classification.NEUTRAL : qualifier.GetClassification();
    }
}
