using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deck : MonoBehaviour
{
    public int deckSize;
    public PlayerController player;
    // Start is called before the first frame update
    void Start()
    {
        deckSize = GameConstants.START_DECK_SIZE;
    }

    public void AssignPlayer(PlayerController p)
    {
        player = p;
        p.deck = this;
    }

    public bool DrawCard()
    {
        if (deckSize > 0)
        {
            deckSize--;
            return true;
        }
        return false;
    }

    public void Reset()
    {
        deckSize = GameConstants.START_DECK_SIZE;
    }
}
