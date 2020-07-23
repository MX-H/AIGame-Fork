using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureCardDescription : CardDescription
{
    public CreatureType creatureType;
    public int attack;
    public int health;
    public SortedSet<KeywordAttributes> attributes;

    public CreatureCardDescription()
    {
        attributes = new SortedSet<KeywordAttributes>();
    }
}
