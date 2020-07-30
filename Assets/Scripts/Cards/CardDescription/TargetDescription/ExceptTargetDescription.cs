using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExceptTargetDescription : ITargettingDescription
{
    IQualifierDescription qualifier;

    public ExceptTargetDescription(TargetType target) : base(target, TargettingType.TARGET)
    {
    }

    public override Classification GetClassification()
    {
        if (qualifier != null)
        {
            switch (qualifier.GetClassification())
            {
                case Classification.POSITIVE:
                    return Classification.NEGATIVE;
                case Classification.NEGATIVE:
                    return Classification.POSITIVE;
            }
        }
        return Classification.NEUTRAL;
    }
}
