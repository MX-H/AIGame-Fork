using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New CreatureModelIndex", menuName = "CreatureModelIndex", order = 51)]

public class CreatureModelIndex : ScriptableObject
{
    [System.Serializable]
    public class StatModelEntry
    {
        public int mana;
        public int health;
        [Range(0, 30)]
        public int value;
        public int total;
    }

    [System.Serializable]
    public class StatProbability
    {
        public StatProbability(int manaCost)
        {
            statModels = new List<StatModelEntry>();
            mana = manaCost;

            int statBudget = PowerBudget.StatBudget(mana);
            for (int i = 0; i < statBudget; i++)
            {
                statModels.Add(new StatModelEntry());
                statModels[i].value = 1;
                statModels[i].health = i + 1;
                statModels[i].total = statBudget;
                statModels[i].mana = mana;
            }
        }

        public void Validate()
        {
            int statBudget = PowerBudget.StatBudget(mana);
            while (statModels.Count > statBudget)
            {
                statModels.RemoveAt(statModels.Count - 1);
            }
            while (statModels.Count < statBudget)
            {
                int i = statModels.Count;

                statModels.Add(new StatModelEntry());
                statModels[i].value = 1;
                statModels[i].health = i + 1;
                statModels[i].mana = mana;
            }

            int total = 0;
            foreach (StatModelEntry statModel in statModels)
            {
                total += statModel.value;
            }

            foreach (StatModelEntry statModel in statModels)
            {
                statModel.total = total;
            }
        }

        [ReadOnly]
        public int mana;
        public List<StatModelEntry> statModels;
    }

    [System.Serializable]
    private class CreatureModelEntry
    {
        public CreatureModelEntry()
        {
            statProbabilities = new List<StatProbability>();
            for (int i = 0; i < 11; i++)
            {
                statProbabilities.Add(new StatProbability(i));
            }
            bodyLambda = 5.0;
        }

        public void Validate()
        {
            foreach (StatProbability stats in statProbabilities)
            {
                stats.Validate();
            }
        }

        public CreatureType type;
        public IHistogram model;
        public CardDescription tokenDesc;
        public double bodyLambda;
        [SerializeField]
        public List<StatProbability> statProbabilities;
    }

    [SerializeField]
    private CreatureModelEntry[] creatureModels;

    CreatureModelIndex()
    {
        CreatureType[] types = (CreatureType[])System.Enum.GetValues(typeof(CreatureType));
        creatureModels = new CreatureModelEntry[types.Length];
        for (int i = 0; i < types.Length; i++)
        {
            creatureModels[i] = new CreatureModelEntry();
            creatureModels[i].type = types[i];
        }
    }

    public void OnValidate()
    {
        foreach (CreatureModelEntry entry in creatureModels)
        {
            entry.Validate();
        }
    }

    public CardDescription GetToken(CreatureType creatureType)
    {
        foreach (CreatureModelEntry entry in creatureModels)
        {
            if (entry.type == creatureType)
            {
                return entry.tokenDesc;
            }
        }

        Debug.LogError("There is no token for type " + creatureType.ToString());
        return new CreatureCardDescription();
    }

    public IHistogram GetModel(CreatureType creatureType)
    {
        foreach (CreatureModelEntry entry in creatureModels)
        {
            if (entry.type == creatureType)
            {
                return entry.model;
            }
        }

        Debug.LogError("There is no card model for type " + creatureType.ToString());
        return new NullHistogram();
    }

    public StatProbability GetStatProbability(CreatureType creatureType, int mana)
    {
        foreach (CreatureModelEntry entry in creatureModels)
        {
            if (entry.type == creatureType)
            {
                return entry.statProbabilities[mana];
            }
        }

        Debug.LogError("There is no stat probabilty for type " + creatureType.ToString() + " and mana " + mana.ToString());
        return new StatProbability(mana);
    }

    public double GetBodyLambda(CreatureType creatureType)
    {
        foreach (CreatureModelEntry entry in creatureModels)
        {
            if (entry.type == creatureType)
            {
                return entry.bodyLambda;
            }
        }

        Debug.LogError("There is no lambda for type " + creatureType.ToString());
        return 0;
    }
}
