using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New DeckModelBank", menuName = "DeckModelBank", order = 51)]
public class DeckModelBank : ScriptableObject
{
    [SerializeField]
    private List<IHistogram> deckModels;

    DeckModelBank()
    {
        deckModels = new List<IHistogram>();
    }

    public int GetSize()
    {
        return deckModels.Count;
    }
    public IHistogram GetDeckModel(int i)
    {
        return deckModels[i];
    }

}