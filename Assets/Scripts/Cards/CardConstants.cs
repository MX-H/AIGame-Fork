using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public static class EffectConstants
{
    public static readonly int MAX_CARD_EFFECTS = 5;
}
public static class PowerBudget
{
    public static readonly double UNIT_COST = 1.0;
    public static readonly double DOWNSIDE_WEIGHT = 1.2;
    public static readonly double FLAT_EFFECT_COST = 0.5;

    public static double ManaFunction(double mana)
    {
        return (-1.0 / 90.0) * mana * mana + (91.0 / 90.0) * mana + 1.5;
    }

    public static int StatBudget(double mana)
    {
        return (int)Math.Round((mana + 0.5) * 2.0, MidpointRounding.AwayFromZero);
    }

    public static double StatsToPowerBudget(int statTotal)
    {
        double manaCost = (statTotal / 2.0) - 0.5;
        return ManaFunction(manaCost);
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

    public static double GetKeywordCost(KeywordAttribute keyword, int atk, int def)
    {
        switch (keyword)
        {
            case KeywordAttribute.NONE:
                return 0;
            case KeywordAttribute.EVASION:
                return (atk / 2.0 + 0.5) * UNIT_COST;
            case KeywordAttribute.FAST_STRIKE:
                return atk / 3.0 * UNIT_COST;
            case KeywordAttribute.PIERCING:
                return 0.5 * (atk / 2.0 + def / 3.0) * UNIT_COST;
            case KeywordAttribute.UNTOUCHABLE:
                return UNIT_COST;
        }

        // If we have not defined a cost function return a really high cost so we don't generate this effect
        Debug.Log("Cost not defined for " + keyword.ToString());
        return ManaFunction(20) * UNIT_COST;
    }

    public static double GetTriggerModifier(TriggerCondition trigger, CardDescription card)
    {
        switch (trigger)
        {
            case TriggerCondition.ON_CREATURE_DIES:
                return 1.5;
            case TriggerCondition.ON_CREATURE_ENTER:
                return 1.5;
            case TriggerCondition.ON_SELF_DIES:
                return 0.9;
            case TriggerCondition.ON_SELF_DAMAGE_DEALT_TO_PLAYER:
                return 1.0;
            case TriggerCondition.ON_SELF_DAMAGE_TAKEN:
                return 1.5;
        }

        return 1.0;
    }
}