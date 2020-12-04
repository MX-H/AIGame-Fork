using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New CreatureCardDescription", menuName = "CreatureCardDescription", order = 52)]
public class CreatureCardDescription : CardDescription
{
    public CreatureType creatureType;
    public int attack;
    public int health;
    private List<KeywordAttribute> attributes;

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

    public List<KeywordAttribute> GetAttributes()
    {
        return attributes;
    }

    public override bool HasKeywordAttribute(KeywordAttribute keyword)
    {
        return attributes.Exists(x => x == keyword);
    }

    public override double PowerLevel()
    {
        double powerLevel = base.PowerLevel();
        foreach (KeywordAttribute keyword in attributes)
        {
            powerLevel += PowerBudget.GetKeywordCost(keyword, attack, health);
        }
        powerLevel += PowerBudget.StatsToPowerBudget(attack + health);
        return powerLevel;
    }
}
