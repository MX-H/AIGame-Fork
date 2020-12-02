using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New CreatureCardDescription", menuName = "CreatureCardDescription", order = 52)]
public class CreatureCardDescription : CardDescription
{
    public CreatureType creatureType;
    public int attack;
    public int health;
    public List<KeywordAttribute> attributes;

    public CreatureCardDescription()
    {
        attributes = new List<KeywordAttribute>();
    }

    public void AddAttribute(KeywordAttribute attribute)
    {
        if (!attributes.Contains(attribute))
        {
            attributes.Add(attribute);
        }
    }
}
