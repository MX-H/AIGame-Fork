using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IQualifierDescription
{
    public readonly QualifierType qualifierType;

    protected IQualifierDescription(QualifierType qualifier)
    {
        qualifierType = qualifier;
    }

    public abstract Classification GetClassification();
}
