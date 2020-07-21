using System.Collections;
using System.Collections.Generic;
using System;

public enum CardType
{
    CREATURE,
    SPELL,
    TRAP
}

public enum TargettingType
{
    ALL_ALLIES,
    ALL_ENEMIES,
    ALL_CREATURES,
    ALL_CARDS,

    TARGET_CREATURE,
    TARGET_TRAP,
    TARGET_CARD,
    TARGET_ON_STACK,

    SELF_PLAYER,
    ENEMY_PLAYER
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
    ON_SELF_DAMAGE_DEALT
}

public enum CreatureType
{
    GOBLIN,
    HUMAN
}

public enum Qualifier
{
    NONE,
    CREATURE_TYPE
}

public enum EffectType
{
    NONE,

    DRAW_CARDS,
    DRAW_RANDOM_CARDS,

    DEAL_DAMAGE,
    SUMMON_TOKEN,
    ANTHEM_EFFECT,
    GRANT_EFFECT
}

public enum ManaCost
{
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

public enum KeywordAttributes
{
    NONE,
    EVASION,
    FAST_STRIKE,
    UNTOUCHABLE,
    PIERCING,
    SURPRISE
}

static class CardEnums
{
    public static System.Type[] EnumTypes = new System.Type[]
        {
            typeof(CardType),
            typeof(TargettingType),
            typeof(CreatureType),
            typeof(Qualifier),
            typeof(EffectType),
            typeof(ManaCost)
        };

    public static EffectType[] NonCreatureEffects = new EffectType[]
        {
            EffectType.DRAW_CARDS,
            EffectType.DRAW_RANDOM_CARDS,
            EffectType.DEAL_DAMAGE,
            EffectType.SUMMON_TOKEN,
            EffectType.ANTHEM_EFFECT
        };
}