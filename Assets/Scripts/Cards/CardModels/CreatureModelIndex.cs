using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New CreatureModelIndex", menuName = "CreatureModelIndex", order = 51)]

public class CreatureModelIndex : ScriptableObject
{
    [System.Serializable]
    private struct CreatureModelEntry
    {
        public CreatureType type;
        public IHistogram model;
        public CardDescription tokenDesc;
    }

    [SerializeField]
    private CreatureModelEntry[] creatureModels;

    CreatureModelIndex()
    {
        CreatureType[] types = (CreatureType[])System.Enum.GetValues(typeof(CreatureType));
        creatureModels = new CreatureModelEntry[types.Length];
        for (int i = 0; i < types.Length; i++)
        {
            creatureModels[i].type = types[i];
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
}
