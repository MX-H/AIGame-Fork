using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiscardPile : MonoBehaviour
{
    // Start is called before the first frame update
    PlayerController player;
    public void AssignPlayer(PlayerController p)
    {
        player = p;
        p.discard = this;
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddCard(Card c)
    {
        c.gameObject.SetActive(false);
        c.transform.SetParent(transform);
    }

    public void AddCreature(Creature c)
    {
        c.gameObject.SetActive(false);
        c.transform.SetParent(transform);
    }
}
