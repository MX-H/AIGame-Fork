using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageEffectDescription : IEffectDescription
{
    public int amount;
    public DamageEffectDescription(bool heal) : base(heal ? EffectType.HEAL_DAMAGE : EffectType.DEAL_DAMAGE)
    {
    }

    public override string CardText()
    {
        return (effectType == EffectType.HEAL_DAMAGE ? "heal(s) " : "take(s) ") + amount.ToString() + " damage";
    }
}
