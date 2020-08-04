using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureTypeQualifierDescription : IQualifierDescription
{
    public bool isNegated;
    public CreatureType creatureType;
    public CreatureTypeQualifierDescription() : base(QualifierType.CREATURE_TYPE)
    {
    }

    public override string CardText(bool plural)
    {
        return (isNegated ? "non-" : "") + CardParsing.Parse(creatureType);
    }

    public override Alignment GetAlignment()
    {
        return isNegated ? Alignment.NEGATIVE : Alignment.POSITIVE;
    }

    public override double PowerLevel()
    {
        return PowerBudget.UNIT_COST * 0.9;
    }
}

public class CreatureQualifierProceduralGenerator : IProceduralQualifierGenerator
{
    public override IQualifierDescription Generate()
    {
        CreatureTypeQualifierDescription desc = new CreatureTypeQualifierDescription();
        desc.creatureType = ProceduralUtils.GetRandomValue<CreatureType>(random, model);

        return desc;
    }

    public override IQualifierDescription GetDescriptionType()
    {
        return new CreatureTypeQualifierDescription();
    }

    public override double GetMinCost()
    {
        return 0.0;
    }
}