using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardTypeQualifierDescription : IQualifierDescription
{
    public CardType cardType;
    public CardTypeQualifierDescription() : base(QualifierType.CARD_TYPE)
    {
    }

    public override string CardText()
    {
        return CardParsing.Parse(cardType);
    }

    public override Alignment GetAlignment()
    {
        return Alignment.NEUTRAL;
    }

    public override double PowerLevel()
    {
        return PowerBudget.UNIT_COST * 0.9;
    }
}

public class CardTypeQualifierProceduralGenerator : IProceduralQualifierGenerator
{
    public override IQualifierDescription Generate()
    {
        CardTypeQualifierDescription desc = new CardTypeQualifierDescription();
        desc.cardType = ProceduralUtils.GetRandomValue<CardType>(random, model);

        return desc;
    }

    public override IQualifierDescription GetDescriptionType()
    {
        return new CardTypeQualifierDescription();
    }

    public override double GetMinCost()
    {
        return 0.0;
    }
}