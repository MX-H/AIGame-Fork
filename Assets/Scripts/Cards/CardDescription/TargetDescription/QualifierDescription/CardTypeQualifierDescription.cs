using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardTypeQualifierDescription : IQualifierDescription
{
    public CardType cardType;
    public CardTypeQualifierDescription(CardType card) : base(QualifierType.CARD_TYPE)
    {
        cardType = card;
    }

    public override Classification GetClassification()
    {
        return Classification.NEUTRAL;
    }
}
