using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PowerBudget
{
    public static readonly double UNIT_COST = 1.0;
    public static readonly double DOWNSIDE_WEIGHT = -1.5;
    public static readonly double ALLOWABLE_MARGIN = 1.25;
    public static readonly double ABSOLUTE_MARGIN = 1.50;

    private static double ManaFunction(int mana)
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