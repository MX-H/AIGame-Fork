using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardDescription : IDescription
{
    public string name;
    public int manaCost;
    public CardType cardType;
    public List<CardEffectDescription> cardEffects = new List<CardEffectDescription>();

    public string CardText(bool plural)
    {
        return "";
    }

    public Alignment GetAlignment()
    {
        return Alignment.NEUTRAL;
    }

    public double PowerLevel()
    {
        double power = 0;
        foreach (CardEffectDescription effect in cardEffects)
        {
            if (effect.GetAlignment() != Alignment.NEGATIVE)
            {
                power += effect.PowerLevel();
            }
            else {
                power -= effect.PowerLevel() * PowerBudget.DOWNSIDE_WEIGHT;
            }
        }
        return power;
    }
}
