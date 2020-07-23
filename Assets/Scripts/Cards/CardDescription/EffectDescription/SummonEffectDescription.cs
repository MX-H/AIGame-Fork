using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SummonEffectDescription : IEffectDescription
{
    public int amount;
    public CreatureType tokenType;
    public SummonEffectDescription() : base(EffectType.SUMMON_TOKEN)
    { }
    public override string CardText()
    {
        return "summon(s) " + amount.ToString() + " " + tokenType.ToString() + " token(s)";
    }
}
