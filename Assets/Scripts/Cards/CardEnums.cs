﻿using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;

// Important do not define values for these enums, it will break how the card histograms work
// since they treat enum values as array indexes

// This also means we cannot remove elements from the enums, replace them with DEPRECATED_X
// Only add elements to the end of the enums

public enum Alignment
{
    POSITIVE,
    NEUTRAL,
    NEGATIVE
}
public enum CardType
{
    CREATURE,
    SPELL,
    TRAP
}

public enum TargetType
{
    CREATURES,
    SET_TRAPS,
    CARDS,
    PLAYERS,

    CREATURE_CARDS,
    SPELL_CARDS,
    TRAP_CARDS,

    PERMANENT,  // Set traps + creatures
    DAMAGEABLE, // Creatures + players

    ACTIVE_TRAPS,
    SPELLS,
    EFFECTS,    // Triggered creature effects
    SPELLS_AND_TRAPS,
    SPELLS_AND_EFFECTS,
    TRAPS_AND_EFFECTS,
    STACK_ITEMS // Spells, traps, and effects
}

public enum TargettingType
{
    SELF,
    FOE,

    ALL,
    ALL_ALLY,
    ALL_ENEMY,

    TARGET,
    TARGET_ALLY,
    TARGET_ENEMY,

    UP_TO_TARGET,
    UP_TO_TARGET_ALLY,
    UP_TO_TARGET_ENEMY,

    EXCEPT,
    EXCEPT_ALLY,
    EXCEPT_ENEMY
}

public enum TriggerCondition
{
    NONE,

    // Enter field
    ON_SELF_ENTER,
    ON_CREATURE_ENTER,

    // On death
    ON_SELF_DIES,
    ON_CREATURE_DIES,

    ON_SELF_DAMAGE_TAKEN,
    ON_SELF_DAMAGE_DEALT_TO_PLAYER,

    ON_STACK_UPDATED,

    IS_ALIVE    // Used for static effects
}

public enum CreatureType
{
    HUMAN,
    GOBLIN,
    FAERIE
}

public enum QualifierType
{
    NONE,
    CARD_TYPE,
    CREATURE_TYPE
}

public enum EffectType
{
    NONE,

    DRAW_CARDS,
    DEAL_DAMAGE,
    HEAL_DAMAGE,
    SUMMON_TOKEN,
    NEGATE,
    DESTROY_CARD,

    GIVE_POSITIVE_STATS,
    AURA_POSITIVE_STATS,
    GIVE_NEGATIVE_STATS,
    AURA_NEGATIVE_STATS,

    GIVE_KEYWORD,
    AURA_KEYWORD,

    GIVE_MANA_COST_REDUCTION,
    AURA_MANA_COST_REDUCTION,
    GIVE_MANA_COST_TAX,
    AURA_MANA_COST_TAX,
}

public enum DrawModifier
{
    SELF,
    OPPONENT,
    RANDOM,
    OPPONENT_RANDOM
}

public enum ManaCost
{
    ZERO,
    ONE,
    TWO,
    THREE,
    FOUR,
    FIVE,
    SIX,
    SEVEN,
    EIGHT,
    NINE,
    TEN
}

public enum KeywordAttribute
{
    NONE,
    EVASION,
    FAST_STRIKE,
    UNTOUCHABLE,
    PIERCING,
    EAGER
}

public enum ModifierType
{
    STAT,
    KEYWORD,
    MANA_COST
}

public enum DurationType
{
    FOREVER,
    AURA,
    END_OF_TURN,
}

static class CardEnums
{
    public static System.Type[] EnumTypes = new System.Type[]
        {
            typeof(CardType),
            typeof(TargetType),
            typeof(TargettingType),
            typeof(CreatureType),
            typeof(QualifierType),
            typeof(EffectType),
            typeof(ManaCost),
            typeof(DrawModifier),
            typeof(KeywordAttribute),
            typeof(TriggerCondition),
            typeof(ModifierType)
        };


    private static Dictionary<(Type T1, Type T2), dynamic> FlagSets;
    private static void RegisterFlagSet<T1, T2>(bool empty = true)
    {
        FlagSets[(typeof(T1), typeof(T2))] = new Dictionary<T1, SortedSet<T2>>();

        // Flags should start out containing all combinations
        if (!empty)
        {
            foreach (T1 t1 in System.Enum.GetValues(typeof(T1)))
            {
                FlagSets[(typeof(T1), typeof(T2))][t1] = new SortedSet<T2>((T2[])System.Enum.GetValues(typeof(T2)));
            }
        }
    }

    public static SortedSet<T> GetValidFlags<T>(IEnumerable<object> keys)
    {
        SortedSet<T> output = new SortedSet<T>((T[])Enum.GetValues(typeof(T)));
        foreach (object key in keys)
        {
            output.IntersectWith(GetValidFlags<T>(key));
        }
        return output;
    }
    public static SortedSet<T> GetValidFlags<T>(dynamic key)
    {
        if (FlagSets.ContainsKey((key.GetType(), typeof(T))))
        {
            if (!FlagSets[(key.GetType(), typeof(T))].ContainsKey(key))
            {
                FlagSets[(key.GetType(), typeof(T))][key] = new SortedSet<T>();
            }
            return new SortedSet<T>(FlagSets[(key.GetType(), typeof(T))][key]);
        }
        Debug.Assert(false, "Flags are undefined for <" + key.GetType().ToString() + "," + typeof(T).ToString() + ">");
        return new SortedSet<T>();
    }

    public static float[] ManaPowerBudgets = new float[]
        {
            5.0f,
            5.0f,
            5.0f,
            5.0f,
            5.0f,
            5.0f,
            5.0f,
            5.0f,
            5.0f,
            5.0f,
            5.0f
        };

    public static Alignment CombineAlignments(Alignment a, Alignment b)
    {
        // Neutral turns everything positive except another neutral

        switch (a)
        {
            case Alignment.NEUTRAL:
                return (b == Alignment.NEUTRAL) ? Alignment.NEUTRAL : Alignment.POSITIVE;
            case Alignment.POSITIVE:
                return (b == Alignment.NEGATIVE) ? Alignment.NEGATIVE : Alignment.POSITIVE;
            case Alignment.NEGATIVE:
                return (b == Alignment.POSITIVE) ? Alignment.NEGATIVE : Alignment.POSITIVE;
        }
        return Alignment.NEUTRAL;
    }

    static CardEnums()
    {
        FlagSets = new Dictionary<(Type T1, Type T2), dynamic>();

        RegisterFlagSet<EffectType, TargetType>();
        RegisterFlagSet<TargetType, EffectType>();

        // Specify specifically when effects can't trigger
        RegisterFlagSet<EffectType, TriggerCondition>(false);
        RegisterFlagSet<TriggerCondition, EffectType>(false);

        RegisterFlagSet<TargetType, TargettingType>();

        RegisterFlagSet<CardType, TriggerCondition>();
        RegisterFlagSet<TriggerCondition, TargettingType>(false);

        RegisterFlagSet<QualifierType, TargetType>();
        RegisterFlagSet<TargetType, QualifierType>();

        RegisterFlagSet<QualifierType, EffectType>();
        RegisterFlagSet<EffectType, QualifierType>();

        // Valid target types for effects
        RegisterFlags(EffectType.DRAW_CARDS, new TargetType[] { TargetType.PLAYERS });
        RegisterFlags(EffectType.DEAL_DAMAGE, new TargetType[] { TargetType.CREATURES, TargetType.PLAYERS, TargetType.DAMAGEABLE });
        RegisterFlags(EffectType.HEAL_DAMAGE, new TargetType[] { TargetType.CREATURES, TargetType.PLAYERS, TargetType.DAMAGEABLE });
        RegisterFlags(EffectType.DESTROY_CARD, new TargetType[] { TargetType.CREATURES, TargetType.SET_TRAPS, TargetType.PERMANENT });
        RegisterFlags(EffectType.SUMMON_TOKEN, new TargetType[] { TargetType.PLAYERS });
        RegisterFlags(EffectType.NEGATE, new TargetType[] { TargetType.SPELLS, TargetType.ACTIVE_TRAPS, TargetType.EFFECTS,
            TargetType.SPELLS_AND_EFFECTS, TargetType.SPELLS_AND_TRAPS, TargetType.TRAPS_AND_EFFECTS, TargetType.STACK_ITEMS });
        RegisterFlags(EffectType.GIVE_POSITIVE_STATS, new TargetType[] { TargetType.CREATURES, TargetType.CREATURE_CARDS });
        RegisterFlags(EffectType.GIVE_NEGATIVE_STATS, new TargetType[] { TargetType.CREATURES });
        RegisterFlags(EffectType.GIVE_KEYWORD, new TargetType[] { TargetType.CREATURES, TargetType.CREATURE_CARDS });
        RegisterFlags(EffectType.GIVE_MANA_COST_REDUCTION, new TargetType[] { TargetType.CARDS, TargetType.CREATURE_CARDS, TargetType.SPELL_CARDS, TargetType.TRAP_CARDS });
        RegisterFlags(EffectType.GIVE_MANA_COST_TAX, new TargetType[] { TargetType.CARDS, TargetType.CREATURE_CARDS, TargetType.SPELL_CARDS, TargetType.TRAP_CARDS });
        RegisterFlags(EffectType.AURA_POSITIVE_STATS, new TargetType[] { TargetType.CREATURES, TargetType.CREATURE_CARDS });
        RegisterFlags(EffectType.AURA_NEGATIVE_STATS, new TargetType[] { TargetType.CREATURES });
        RegisterFlags(EffectType.AURA_KEYWORD, new TargetType[] { TargetType.CREATURES, TargetType.CREATURE_CARDS });
        RegisterFlags(EffectType.AURA_MANA_COST_REDUCTION, new TargetType[] { TargetType.CARDS, TargetType.CREATURE_CARDS, TargetType.SPELL_CARDS, TargetType.TRAP_CARDS });
        RegisterFlags(EffectType.AURA_MANA_COST_TAX, new TargetType[] { TargetType.CARDS, TargetType.CREATURE_CARDS, TargetType.SPELL_CARDS, TargetType.TRAP_CARDS });

        // Valid trigger conditions to perform effects
        RemoveFlags(EffectType.DRAW_CARDS, new TriggerCondition[] { TriggerCondition.IS_ALIVE });
        RemoveFlags(EffectType.DEAL_DAMAGE, new TriggerCondition[] { TriggerCondition.IS_ALIVE, TriggerCondition.ON_SELF_DAMAGE_TAKEN });
        RemoveFlags(EffectType.HEAL_DAMAGE, new TriggerCondition[] { TriggerCondition.IS_ALIVE });
        RemoveFlags(EffectType.DESTROY_CARD, new TriggerCondition[] { TriggerCondition.IS_ALIVE });
        RemoveFlags(EffectType.SUMMON_TOKEN, new TriggerCondition[] { TriggerCondition.IS_ALIVE, TriggerCondition.ON_CREATURE_ENTER, TriggerCondition.ON_CREATURE_DIES });
        RemoveFlags(EffectType.NEGATE,  ((TriggerCondition[])Enum.GetValues(typeof(TriggerCondition))).Except(new TriggerCondition[] { TriggerCondition.ON_STACK_UPDATED }));
        RemoveFlags(EffectType.GIVE_POSITIVE_STATS, new TriggerCondition[] { TriggerCondition.IS_ALIVE });
        RemoveFlags(EffectType.GIVE_NEGATIVE_STATS, new TriggerCondition[] { TriggerCondition.IS_ALIVE });
        RemoveFlags(EffectType.GIVE_KEYWORD, new TriggerCondition[] { TriggerCondition.IS_ALIVE });
        RemoveFlags(EffectType.GIVE_MANA_COST_REDUCTION, new TriggerCondition[] { TriggerCondition.IS_ALIVE });
        RemoveFlags(EffectType.GIVE_MANA_COST_TAX, new TriggerCondition[] { TriggerCondition.IS_ALIVE });
        RemoveFlags(EffectType.AURA_POSITIVE_STATS, ((TriggerCondition[])Enum.GetValues(typeof(TriggerCondition))).Except(new TriggerCondition[] { TriggerCondition.IS_ALIVE }));
        RemoveFlags(EffectType.AURA_NEGATIVE_STATS, ((TriggerCondition[])Enum.GetValues(typeof(TriggerCondition))).Except(new TriggerCondition[] { TriggerCondition.IS_ALIVE }));
        RemoveFlags(EffectType.AURA_KEYWORD, ((TriggerCondition[])Enum.GetValues(typeof(TriggerCondition))).Except(new TriggerCondition[] { TriggerCondition.IS_ALIVE }));
        RemoveFlags(EffectType.AURA_MANA_COST_REDUCTION, ((TriggerCondition[])Enum.GetValues(typeof(TriggerCondition))).Except(new TriggerCondition[] { TriggerCondition.IS_ALIVE }));
        RemoveFlags(EffectType.AURA_MANA_COST_TAX, ((TriggerCondition[])Enum.GetValues(typeof(TriggerCondition))).Except(new TriggerCondition[] { TriggerCondition.IS_ALIVE }));

        // Valid targetting types for targets
        RegisterFlags(TargetType.CREATURES, new TargettingType[] { TargettingType.ALL, TargettingType.ALL_ALLY, TargettingType.ALL_ENEMY, TargettingType.TARGET, TargettingType.TARGET_ALLY, TargettingType.TARGET_ENEMY,
            TargettingType.UP_TO_TARGET, TargettingType.UP_TO_TARGET_ALLY, TargettingType.UP_TO_TARGET_ENEMY, TargettingType.EXCEPT, TargettingType.EXCEPT_ALLY, TargettingType.EXCEPT_ENEMY });
        RegisterFlags(TargetType.SET_TRAPS, new TargettingType[] { TargettingType.ALL, TargettingType.ALL_ALLY, TargettingType.ALL_ENEMY, TargettingType.TARGET, TargettingType.TARGET_ALLY, TargettingType.TARGET_ENEMY,
            TargettingType.UP_TO_TARGET, TargettingType.UP_TO_TARGET_ALLY, TargettingType.UP_TO_TARGET_ENEMY, TargettingType.EXCEPT, TargettingType.EXCEPT_ALLY, TargettingType.EXCEPT_ENEMY });
        RegisterFlags(TargetType.CARDS, new TargettingType[] { TargettingType.TARGET_ALLY, TargettingType.UP_TO_TARGET_ALLY, TargettingType.ALL, TargettingType.ALL_ALLY, TargettingType.ALL_ENEMY });
        RegisterFlags(TargetType.PLAYERS, new TargettingType[] { TargettingType.SELF, TargettingType.FOE, TargettingType.ALL, TargettingType.TARGET });
        RegisterFlags(TargetType.CREATURE_CARDS, new TargettingType[] { TargettingType.TARGET_ALLY, TargettingType.UP_TO_TARGET_ALLY, TargettingType.ALL, TargettingType.ALL_ALLY, TargettingType.ALL_ENEMY });
        RegisterFlags(TargetType.SPELL_CARDS, new TargettingType[] { TargettingType.TARGET_ALLY, TargettingType.UP_TO_TARGET_ALLY, TargettingType.ALL, TargettingType.ALL_ALLY, TargettingType.ALL_ENEMY });
        RegisterFlags(TargetType.TRAP_CARDS, new TargettingType[] { TargettingType.TARGET_ALLY, TargettingType.UP_TO_TARGET_ALLY, TargettingType.ALL, TargettingType.ALL_ALLY, TargettingType.ALL_ENEMY });
        RegisterFlags(TargetType.PERMANENT, new TargettingType[] { TargettingType.ALL, TargettingType.ALL_ALLY, TargettingType.ALL_ENEMY, TargettingType.TARGET, TargettingType.TARGET_ALLY, TargettingType.TARGET_ENEMY,
            TargettingType.UP_TO_TARGET, TargettingType.UP_TO_TARGET_ALLY, TargettingType.UP_TO_TARGET_ENEMY, TargettingType.EXCEPT, TargettingType.EXCEPT_ALLY, TargettingType.EXCEPT_ENEMY });
        RegisterFlags(TargetType.DAMAGEABLE, new TargettingType[] { TargettingType.ALL, TargettingType.ALL_ALLY, TargettingType.ALL_ENEMY, TargettingType.TARGET, TargettingType.TARGET_ALLY, TargettingType.TARGET_ENEMY,
            TargettingType.UP_TO_TARGET, TargettingType.UP_TO_TARGET_ALLY, TargettingType.UP_TO_TARGET_ENEMY });
        RegisterFlags(TargetType.ACTIVE_TRAPS, new TargettingType[] { TargettingType.ALL, TargettingType.ALL_ALLY, TargettingType.ALL_ENEMY, TargettingType.TARGET, TargettingType.TARGET_ALLY, TargettingType.TARGET_ENEMY,
            TargettingType.UP_TO_TARGET, TargettingType.UP_TO_TARGET_ALLY, TargettingType.UP_TO_TARGET_ENEMY });
        RegisterFlags(TargetType.SPELLS, new TargettingType[] { TargettingType.ALL, TargettingType.TARGET });
        RegisterFlags(TargetType.EFFECTS, new TargettingType[] { TargettingType.ALL, TargettingType.TARGET });
        RegisterFlags(TargetType.SPELLS_AND_TRAPS, new TargettingType[] { TargettingType.ALL, TargettingType.TARGET });
        RegisterFlags(TargetType.SPELLS_AND_EFFECTS, new TargettingType[] { TargettingType.ALL, TargettingType.TARGET });
        RegisterFlags(TargetType.TRAPS_AND_EFFECTS, new TargettingType[] { TargettingType.ALL, TargettingType.TARGET });
        RegisterFlags(TargetType.STACK_ITEMS, new TargettingType[] { TargettingType.ALL, TargettingType.TARGET });

        // Valid trigger conditions for card types
        RegisterFlags(CardType.CREATURE, new TriggerCondition[] { TriggerCondition.ON_SELF_ENTER, TriggerCondition.ON_CREATURE_ENTER, TriggerCondition.ON_SELF_DIES,
            TriggerCondition.ON_CREATURE_DIES, TriggerCondition.ON_SELF_DAMAGE_TAKEN, TriggerCondition.ON_SELF_DAMAGE_DEALT_TO_PLAYER, TriggerCondition.IS_ALIVE});
        RegisterFlags(CardType.SPELL, new TriggerCondition[] { TriggerCondition.NONE });
        RegisterFlags(CardType.TRAP, new TriggerCondition[] { TriggerCondition.NONE, TriggerCondition.ON_STACK_UPDATED });

        // Targetting types for specific triggers
        RemoveFlags(TriggerCondition.ON_SELF_DAMAGE_DEALT_TO_PLAYER, new TargettingType[] { TargettingType.TARGET, TargettingType.UP_TO_TARGET });
        RemoveFlags(TriggerCondition.ON_SELF_DAMAGE_TAKEN, new TargettingType[] { TargettingType.TARGET, TargettingType.UP_TO_TARGET });
        RemoveFlags(TriggerCondition.IS_ALIVE, ((TargettingType[])Enum.GetValues(typeof(TargettingType))).Except(new TargettingType[] { TargettingType.ALL, TargettingType.ALL_ALLY, TargettingType.ALL_ENEMY }));

        RegisterFlags(QualifierType.NONE, (TargetType[])Enum.GetValues(typeof(TargetType)));
        RegisterFlags(QualifierType.CREATURE_TYPE, new TargetType[] { TargetType.CREATURES, TargetType.CREATURE_CARDS });
        RegisterFlags(QualifierType.CARD_TYPE, new TargetType[] { TargetType.CARDS });

        RegisterFlags(QualifierType.NONE, (EffectType[])Enum.GetValues(typeof(EffectType)));
        RegisterFlags(QualifierType.CREATURE_TYPE, new EffectType[] { EffectType.DRAW_CARDS });
        RegisterFlags(QualifierType.CARD_TYPE, new EffectType[] { EffectType.DRAW_CARDS });
    }

    static private void RegisterFlags<T1, T2>(T1 key, IEnumerable<T2> flags, bool storeReverse = true)
    {
        if (FlagSets.ContainsKey((typeof(T1), typeof(T2))))
        {
            if (FlagSets[(typeof(T1), typeof(T2))].ContainsKey(key))
            {
                FlagSets[(typeof(T1), typeof(T2))][key].UnionWith(flags);
            }
            else
            {
                FlagSets[(typeof(T1), typeof(T2))][key] = new SortedSet<T2>(flags);
            }
        }

        if (storeReverse && FlagSets.ContainsKey((typeof(T2), typeof(T1))))
        {
            foreach (T2 f in flags)
            {
                if (!FlagSets[(typeof(T2), typeof(T1))].ContainsKey(f))
                {
                    FlagSets[(typeof(T2), typeof(T1))][f] = new SortedSet<T1>();
                }
                FlagSets[(typeof(T2), typeof(T1))][f].Add(key);
            }
        }
    }

    // Sometimes its easier to define flags by what something can't do
    static private void RemoveFlags<T1, T2>(T1 key, IEnumerable<T2> flags, bool storeReverse = true)
    {
        if (FlagSets.ContainsKey((typeof(T1), typeof(T2))))
        {
            // If set doesn't exist assume it contains all but the listed flags
            if (!FlagSets[(typeof(T1), typeof(T2))].ContainsKey(key))
            {
                FlagSets[(typeof(T1), typeof(T2))][key] = new SortedSet<T2>((T2[])System.Enum.GetValues(typeof(T2)));
            }

            FlagSets[(typeof(T1), typeof(T2))][key].ExceptWith(flags);
        }

        if (storeReverse && FlagSets.ContainsKey((typeof(T2), typeof(T1))))
        {
            foreach (T2 f in flags)
            {
                if (!FlagSets[(typeof(T2), typeof(T1))].ContainsKey(f))
                {
                    FlagSets[(typeof(T2), typeof(T1))][f] = new SortedSet<T1>();
                }
                FlagSets[(typeof(T2), typeof(T1))][f].Remove(key);
            }
        }
    }
}