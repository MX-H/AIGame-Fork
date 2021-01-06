using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEngine.Assertions;

public static class ProceduralUtils
{
    static private Dictionary<EffectType, IProceduralEffectGenerator> proceduralEffectGenerators;
    static private Dictionary<TargettingType, IProceduralTargettingGenerator> proceduralTargettingGenerators;
    static private Dictionary<QualifierType, IProceduralQualifierGenerator> proceduralQualifierGenerators;

    static private List<Tuple<double, EffectType>> effectMinCosts;
    static private List<Tuple<double, TargettingType>> targettingMinCosts;
    static private List<Tuple<double, QualifierType>> qualifierMinCosts;

    static private Dictionary<Alignment, SortedSet<EffectType>> effectsByAlignment;
    static private Dictionary<Alignment, SortedSet<TargettingType>> targettingByAlignment;
    static private Dictionary<Alignment, SortedSet<QualifierType>> qualifiersByAlignment;

    static private void RegisterProceduralGenerator(EffectType type, IProceduralEffectGenerator generator)
    {
        Assert.IsFalse(proceduralEffectGenerators.ContainsKey(type));
        Assert.AreEqual(type, generator.GetEffectType());
        proceduralEffectGenerators[type] = generator;

        double minCost = generator.GetMinCost();
        int i;
        for (i = 0; i < effectMinCosts.Count; i++)
        {
            if (minCost <= effectMinCosts[i].Item1)
            {
                break;
            }
        }
        effectMinCosts.Insert(i, new Tuple<double, EffectType>(minCost, type));
        effectsByAlignment[generator.GetDescriptionType().GetAlignment()].Add(type);
    }
    static private void RegisterProceduralGenerator(TargettingType type, IProceduralTargettingGenerator generator)
    {
        Assert.IsFalse(proceduralTargettingGenerators.ContainsKey(type));
        Assert.AreEqual(type, generator.GetTargettingType());
        proceduralTargettingGenerators[type] = generator;

        double minCost = generator.GetMinCost();
        int i;
        for (i = 0; i < targettingMinCosts.Count; i++)
        {
            if (minCost <= targettingMinCosts[i].Item1)
            {
                break;
            }
        }
        targettingMinCosts.Insert(i, new Tuple<double, TargettingType>(minCost, type));
        targettingByAlignment[generator.GetDescriptionType().GetAlignment()].Add(type);
    }

    static private void RegisterProceduralGenerator(QualifierType type, IProceduralQualifierGenerator generator)
    {
        Assert.IsFalse(proceduralQualifierGenerators.ContainsKey(type));
        Assert.AreEqual(type, generator.GetQualifierType());
        proceduralQualifierGenerators[type] = generator;

        double minCost = generator.GetMinCost();
        int i;
        for (i = 0; i < qualifierMinCosts.Count; i++)
        {
            if (minCost <= qualifierMinCosts[i].Item1)
            {
                break;
            }
        }
        qualifierMinCosts.Insert(i, new Tuple<double, QualifierType>(minCost, type));
        qualifiersByAlignment[generator.GetDescriptionType().GetAlignment()].Add(type);
    }

    static public IProceduralEffectGenerator GetProceduralGenerator(EffectType type)
    {
        if (proceduralEffectGenerators.ContainsKey(type))
        {
            return proceduralEffectGenerators[type];
        }
        Assert.IsTrue(false);
        return null;
    }
    static public IProceduralTargettingGenerator GetProceduralGenerator(TargettingType type)
    {
        if (proceduralTargettingGenerators.ContainsKey(type))
        {
            return proceduralTargettingGenerators[type];
        }
        Assert.IsTrue(false);
        return null;
    }

    static public IProceduralQualifierGenerator GetProceduralGenerator(QualifierType type)
    {
        if (proceduralQualifierGenerators.ContainsKey(type))
        {
            return proceduralQualifierGenerators[type];
        }
        Assert.IsTrue(false);
        return null;
    }

    // Keywords have varying evaluations depending on the budget
    static public SortedSet<KeywordAttribute> GetKeywordsWithinBudget(double maxBudget, int atk, int hp)
    {
        SortedSet<KeywordAttribute> ret = new SortedSet<KeywordAttribute>();

        foreach (KeywordAttribute keyword in Enum.GetValues(typeof(KeywordAttribute)))
        {
            if (PowerBudget.GetKeywordCost(keyword, atk, hp) <= maxBudget)
            {
                ret.Add(keyword);
            }
        }

        return ret;
    }

    static public SortedSet<EffectType> GetEffectsWithinBudget(double maxBudget)
    {
        SortedSet<EffectType> ret = new SortedSet<EffectType>();
        foreach (Tuple<double, EffectType> pair in effectMinCosts)
        {
            if (pair.Item1 > maxBudget)
            {
                break;
            }
            ret.Add(pair.Item2);
        }
        return ret;
    }

    static public SortedSet<TargettingType> GetTargettingWithinBudget(double maxBudget)
    {
        SortedSet<TargettingType> ret = new SortedSet<TargettingType>();
        foreach (Tuple<double, TargettingType> pair in targettingMinCosts)
        {
            if (pair.Item1 > maxBudget)
            {
                break;
            }
            ret.Add(pair.Item2);
        }
        return ret;
    }

    static public SortedSet<QualifierType> GetQualifiersWithinBudget(double maxBudget)
    {
        SortedSet<QualifierType> ret = new SortedSet<QualifierType>();
        foreach (Tuple<double, QualifierType> pair in qualifierMinCosts)
        {
            if (pair.Item1 > maxBudget)
            {
                break;
            }
            ret.Add(pair.Item2);
        }
        return ret;
    }

    static public SortedSet<EffectType> GetEffectsByAlignment(Alignment a)
    {
        return new SortedSet<EffectType>(effectsByAlignment[a]);
    }

    static public SortedSet<TargettingType> GetTargettingByAlignment(Alignment a)
    {
        return new SortedSet<TargettingType>(targettingByAlignment[a]);
    }

    static public SortedSet<QualifierType> GetQualifiersByAlignment(Alignment a)
    {
        return new SortedSet<QualifierType>(qualifiersByAlignment[a]);
    }

    static public int GetUpperBound(IDescription desc, ref int field, int min, int max, double budget)
    {
        int originalVal = field;

        // Find the lower bound of card amount
        int bound;
        for (bound = max; bound > min; bound--)
        {
            field = bound;
            if (desc.PowerLevel() <= budget)
            {
                break;
            }
        }

        field = originalVal;

        return bound;
    }

    static public int GetLowerBound(IDescription desc, ref int field, int min, int max, double budget)
    {
        int originalVal = field;

        // Find the lower bound of card amount
        int bound;
        for (bound = min; bound < max; bound++)
        {
            field = bound;
            if (desc.PowerLevel() >= budget)
            {
                break;
            }
        }

        field = originalVal;
        return bound;
    }

    static public int GetUpperBound(IDescription desc, ref int field1, ref int field2, int min, int max, double budget)
    {
        int original1 = field1;
        int original2 = field2;

        // Find the lower bound of card amount
        int bound;
        for (bound = max; bound > min; bound--)
        {
            field1 = bound;
            field2 = bound;
            if (desc.PowerLevel() <= budget)
            {
                break;
            }
        }

        field1 = original1;
        field2 = original2;

        return bound;
    }
    static public int GetLowerBound(IDescription desc, ref int field1, ref int field2, int min, int max, double budget)
    {
        int original1 = field1;
        int original2 = field2;

        // Find the lower bound of card amount
        int bound;
        for (bound = min; bound < max; bound++)
        {
            field1 = bound;
            field2 = bound;
            if (desc.PowerLevel() >= budget)
            {
                break;
            }
        }

        field1 = original1;
        field2 = original2;

        return bound;
    }

    static public T GetRandomValue<T>(System.Random random, IHistogram model) where T : Enum
    {
        return GetRandomValue<T>(random, model, (T[])Enum.GetValues(typeof(T)));
    }
    static public T GetRandomValue<T>(System.Random random, IHistogram model, IEnumerable<T> whitelist) where T : Enum
    {
        int result = random.Next(model.GetTotal<T>(whitelist));
        foreach (T t in whitelist)
        {
            result -= model.GetValue<T>(t);
            if (result < 0)
            {
                return t;
            }
        }

        // Should never get here
        Debug.Log("Wasn't able to generate a random value for " + typeof(T).ToString() + ", returning first value");
        return (T)Enum.GetValues(typeof(T)).GetValue(0);
    }
    static public T GetRandomValue<T>(System.Random random, IHistogram model, IEnumerable<T> whitelist, IEnumerable<T> list) where T : Enum
    {
        whitelist = whitelist.Union(list);
        int result = random.Next(model.GetTotal<T>(whitelist));
        foreach (T t in whitelist)
        {
            result -= model.GetValue<T>(t);
            if (result < 0)
            {
                return t;
            }
        }

        // Should never get here
        Debug.Log("Wasn't able to generate a random value for " + typeof(T).ToString() + ", returning first value");
        return (T)Enum.GetValues(typeof(T)).GetValue(0);
    }

    static public bool FlagsExist<T>(IEnumerable<T> blacklist, IEnumerable<T> whitelist)
    {
        IEnumerable<T> result = whitelist.Except(blacklist);
        return result.Count() > 0;
    }

    static public T GetRandomValueExcluding<T>(System.Random random, IHistogram model, IEnumerable<T> blacklist) where T : Enum
    {
        return GetRandomValueExcluding<T>(random, model, blacklist, (T[])Enum.GetValues(typeof(T)));
    }
    static public T GetRandomValueExcluding<T>(System.Random random, IHistogram model, IEnumerable<T> blacklist, IEnumerable<T> whitelist) where T : Enum
    {
        whitelist = whitelist.Except(blacklist);
        int result = random.Next(model.GetTotal<T>(whitelist));
        foreach (T t in whitelist)
        {
            result -= model.GetValue<T>(t);
            if (result < 0)
            {
                return t;
            }
        }

        // Should never get here
        Debug.Log("Wasn't able to generate a random value for " + typeof(T).ToString() + ", returning first value");
        return (T)Enum.GetValues(typeof(T)).GetValue(0);
    }

    static public T GetRandomValueIntersect<T>(System.Random random, IHistogram model, IEnumerable<T> whitelist, IEnumerable<T> list) where T : Enum
    {
        whitelist = whitelist.Intersect(list);
        int result = random.Next(model.GetTotal<T>(whitelist));
        foreach (T t in whitelist)
        {
            result -= model.GetValue<T>(t);
            if (result < 0)
            {
                return t;
            }
        }

        // Should never get here
        Debug.Log("Wasn't able to generate a random value for " + typeof(T).ToString() + ", returning first value");
        return (T)Enum.GetValues(typeof(T)).GetValue(0);
    }

    public static Texture2D GetRandomTexture(System.Random random, ImageBank ib)
    {
        return ib.GetTexture(random.Next(ib.GetSize()));
    }

    static ProceduralUtils()
    {
        proceduralEffectGenerators = new Dictionary<EffectType, IProceduralEffectGenerator>();
        proceduralQualifierGenerators = new Dictionary<QualifierType, IProceduralQualifierGenerator>();
        proceduralTargettingGenerators = new Dictionary<TargettingType, IProceduralTargettingGenerator>();

        effectMinCosts = new List<Tuple<double, EffectType>>();
        targettingMinCosts = new List<Tuple<double, TargettingType>>();
        qualifierMinCosts = new List<Tuple<double, QualifierType>>();

        effectsByAlignment = new Dictionary<Alignment, SortedSet<EffectType>>();
        targettingByAlignment = new Dictionary<Alignment, SortedSet<TargettingType>>();
        qualifiersByAlignment = new Dictionary<Alignment, SortedSet<QualifierType>>();
        foreach (Alignment a in Enum.GetValues(typeof(Alignment)))
        {
            effectsByAlignment[a] = new SortedSet<EffectType>();
            targettingByAlignment[a] = new SortedSet<TargettingType>();
            qualifiersByAlignment[a] = new SortedSet<QualifierType>();
        }
        qualifiersByAlignment[Alignment.NEUTRAL].Add(QualifierType.NONE);
        qualifierMinCosts.Add(new Tuple<double, QualifierType>(0, QualifierType.NONE));

        RegisterProceduralGenerator(EffectType.DRAW_CARDS, new DrawEffectProceduralGenerator());
        RegisterProceduralGenerator(EffectType.DEAL_DAMAGE, new DamageEffectProceduralGenerator(false));
        RegisterProceduralGenerator(EffectType.HEAL_DAMAGE, new DamageEffectProceduralGenerator(true));
        RegisterProceduralGenerator(EffectType.NEGATE, new NegateEffectProceduralGenerator());
        RegisterProceduralGenerator(EffectType.SUMMON_TOKEN, new SummonEffectProceduralGenerator());
        RegisterProceduralGenerator(EffectType.GIVE_POSITIVE_STATS, new GiveModifierEffectProceduralGenerator(EffectType.GIVE_POSITIVE_STATS));
        RegisterProceduralGenerator(EffectType.GIVE_NEGATIVE_STATS, new GiveModifierEffectProceduralGenerator(EffectType.GIVE_NEGATIVE_STATS));
        RegisterProceduralGenerator(EffectType.GIVE_KEYWORD, new GiveModifierEffectProceduralGenerator(EffectType.GIVE_KEYWORD));
        RegisterProceduralGenerator(EffectType.GIVE_MANA_COST_REDUCTION, new GiveModifierEffectProceduralGenerator(EffectType.GIVE_MANA_COST_REDUCTION));
        RegisterProceduralGenerator(EffectType.GIVE_MANA_COST_TAX, new GiveModifierEffectProceduralGenerator(EffectType.GIVE_MANA_COST_TAX));
        RegisterProceduralGenerator(EffectType.AURA_POSITIVE_STATS, new AuraModifierEffectProceduralGenerator(EffectType.AURA_POSITIVE_STATS));
        RegisterProceduralGenerator(EffectType.AURA_NEGATIVE_STATS, new AuraModifierEffectProceduralGenerator(EffectType.AURA_NEGATIVE_STATS));
        RegisterProceduralGenerator(EffectType.AURA_KEYWORD, new AuraModifierEffectProceduralGenerator(EffectType.AURA_KEYWORD));
        RegisterProceduralGenerator(EffectType.AURA_MANA_COST_REDUCTION, new AuraModifierEffectProceduralGenerator(EffectType.AURA_MANA_COST_REDUCTION));
        RegisterProceduralGenerator(EffectType.AURA_MANA_COST_TAX, new AuraModifierEffectProceduralGenerator(EffectType.AURA_MANA_COST_TAX));

        RegisterProceduralGenerator(TargettingType.SELF, new SelfTargettingProceduralGenerator());
        RegisterProceduralGenerator(TargettingType.FOE, new FoeTargettingProceduralGenerator());
        RegisterProceduralGenerator(TargettingType.TARGET, new TargetXProceduralGenerator(Alignment.NEUTRAL));
        RegisterProceduralGenerator(TargettingType.TARGET_ALLY, new TargetXProceduralGenerator(Alignment.POSITIVE));
        RegisterProceduralGenerator(TargettingType.TARGET_ENEMY, new TargetXProceduralGenerator(Alignment.NEGATIVE));
        RegisterProceduralGenerator(TargettingType.UP_TO_TARGET, new UpToXProceduralGenerator(Alignment.NEUTRAL));
        RegisterProceduralGenerator(TargettingType.UP_TO_TARGET_ALLY, new UpToXProceduralGenerator(Alignment.POSITIVE));
        RegisterProceduralGenerator(TargettingType.UP_TO_TARGET_ENEMY, new UpToXProceduralGenerator(Alignment.NEGATIVE));
        RegisterProceduralGenerator(TargettingType.ALL, new AllTargetProceduralGenerator(Alignment.NEUTRAL));
        RegisterProceduralGenerator(TargettingType.ALL_ALLY, new AllTargetProceduralGenerator(Alignment.POSITIVE));
        RegisterProceduralGenerator(TargettingType.ALL_ENEMY, new AllTargetProceduralGenerator(Alignment.NEGATIVE));
        RegisterProceduralGenerator(TargettingType.EXCEPT, new ExceptTargetProceduralGenerator(Alignment.NEUTRAL));
        RegisterProceduralGenerator(TargettingType.EXCEPT_ALLY, new ExceptTargetProceduralGenerator(Alignment.POSITIVE));
        RegisterProceduralGenerator(TargettingType.EXCEPT_ENEMY, new ExceptTargetProceduralGenerator(Alignment.NEGATIVE));

        RegisterProceduralGenerator(QualifierType.CREATURE_TYPE, new CreatureQualifierProceduralGenerator());
        RegisterProceduralGenerator(QualifierType.CARD_TYPE, new CardTypeQualifierProceduralGenerator());
    }
}
