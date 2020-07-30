using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureTypeQualifierDescription : IQualifierDescription
{
    public bool isNegated;
    public CreatureType creatureType;
    public CreatureTypeQualifierDescription(CreatureType creature, bool negate) : base(QualifierType.CREATURE_TYPE)
    {
        creatureType = creature;
        isNegated = negate;
    }

    public override Classification GetClassification()
    {
        return isNegated ? Classification.NEGATIVE : Classification.POSITIVE;
    }
}
