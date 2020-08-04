using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static class EffectConstants
{
    public static readonly int MAX_CARD_EFFECTS = 5;
}
public static class PowerBudget
{
    public static readonly double UNIT_COST = 1.0;
    public static readonly double DOWNSIDE_WEIGHT = 1.2;
    public static readonly double FLAT_EFFECT_COST = 0.5;

    private static double ManaFunction(double mana)
    {
        return (-1.0 / 90.0) * mana * mana + (91.0 / 90.0) * mana + 1.5;
    }

    public static readonly double[] ManaPowerBudgets = new double[]
    {
        ManaFunction(0) * UNIT_COST,
        ManaFunction(1) * UNIT_COST,
        ManaFunction(2) * UNIT_COST,
        ManaFunction(3) * UNIT_COST,
        ManaFunction(4) * UNIT_COST,
        ManaFunction(5) * UNIT_COST,
        ManaFunction(6) * UNIT_COST,
        ManaFunction(7) * UNIT_COST,
        ManaFunction(8) * UNIT_COST,
        ManaFunction(9) * UNIT_COST,
        ManaFunction(10) * UNIT_COST,
    };

    public static readonly double[] ManaPowerMargin = new double[]
    {
            ManaFunction(0.5) * UNIT_COST,
            ManaFunction(1.5) * UNIT_COST,
            ManaFunction(2.5) * UNIT_COST,
            ManaFunction(3.5) * UNIT_COST,
            ManaFunction(4.5) * UNIT_COST,
            ManaFunction(5.5) * UNIT_COST,
            ManaFunction(6.5) * UNIT_COST,
            ManaFunction(7.5) * UNIT_COST,
            ManaFunction(8.5) * UNIT_COST,
            ManaFunction(9.5) * UNIT_COST,
            ManaFunction(10.5) * UNIT_COST,
    };

    public static readonly double[] ManaPowerLimit = new double[]
    {
            ManaFunction(1) * UNIT_COST,
            ManaFunction(2) * UNIT_COST,
            ManaFunction(3) * UNIT_COST,
            ManaFunction(4) * UNIT_COST,
            ManaFunction(5) * UNIT_COST,
            ManaFunction(6) * UNIT_COST,
            ManaFunction(7) * UNIT_COST,
            ManaFunction(8) * UNIT_COST,
            ManaFunction(9) * UNIT_COST,
            ManaFunction(10) * UNIT_COST,
            ManaFunction(11) * UNIT_COST,
    };

    public static double GetEffectCost(EffectType e)
    {
        switch (e)
        {
            case EffectType.DEAL_DAMAGE:
                return UNIT_COST;
            case EffectType.HEAL_DAMAGE:
                return UNIT_COST / 2.5;
            case EffectType.DRAW_CARDS:
                return UNIT_COST * 2.5;
            case EffectType.NEGATE:
                return UNIT_COST * 3;
            case EffectType.SUMMON_TOKEN:
                return UNIT_COST;
        }

        Debug.Log("Cost not defined for " + e.ToString());
        return UNIT_COST;
    }
}