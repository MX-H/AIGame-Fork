using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AllTargetDescription : ITargettingDescription
{
    IQualifierDescription qualifier;

    public AllTargetDescription(TargetType target) : base(target, TargettingType.TARGET)
    {
    }

    public override Classification GetClassification()
    {
        return (qualifier == null) ? Classification.NEUTRAL : qualifier.GetClassification();
    }
}
