using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New ImageGlossary", menuName = "ImageGlossary", order = 51)]
public class ImageGlossary : ScriptableObject
{
    [System.Serializable]
    private struct CreatureImageVal
    {
        public CreatureType type;
        public ImageBank bank;
    }


    [SerializeField]
    private ImageBank spellImageBank;

    [SerializeField]
    private ImageBank trapImageBank;

    [SerializeField]
    private CreatureImageVal[] creatureImageBanks;

    [SerializeField]
    private ImageBank defaultImageBank;

    ImageGlossary()
    {
        CreatureType[] types = (CreatureType[])System.Enum.GetValues(typeof(CreatureType));
        creatureImageBanks = new CreatureImageVal[types.Length];
        for (int i = 0; i < types.Length; i++)
        {
            creatureImageBanks[i].type = types[i];
        }
    }

    public ImageBank GetSpellImages()
    {
        return spellImageBank;
    }

    public ImageBank GetTrapImages()
    {
        return trapImageBank;
    }
    public ImageBank GetCreatureImages(CreatureType c)
    {
        foreach (CreatureImageVal v in creatureImageBanks)
        {
            if (v.type == c)
            {
                return v.bank;
            }
        }
        return defaultImageBank;
    }

}
