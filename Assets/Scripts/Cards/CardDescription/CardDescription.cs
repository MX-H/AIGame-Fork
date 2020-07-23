using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardDescription
{
    public string name;
    public int manaCost;
    public CardType cardType;
    public List<CardEffectDescription> cardEffects = new List<CardEffectDescription>();
}
