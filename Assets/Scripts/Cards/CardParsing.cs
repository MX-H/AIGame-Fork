using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static public class CardParsing
{
    static private readonly string PARSE_ERROR = "MISSING STRING";

    static public string Parse(KeywordAttribute a)
    {
        switch (a)
        {
            case KeywordAttribute.EVASION:
                return "Evasion";
            case KeywordAttribute.FAST_STRIKE:
                return "Swift Strike";
            case KeywordAttribute.PIERCING:
                return "Excessive";
            case KeywordAttribute.SURPRISE:
                return "Surprise";
            case KeywordAttribute.UNTOUCHABLE:
                return "Untouchable";
        }
        return PARSE_ERROR;
    }

    static public string Parse(TriggerCondition t)
    {
        switch (t)
        {
            case TriggerCondition.ON_SELF_ENTER:
                return "Arrival";
            case TriggerCondition.ON_CREATURE_ENTER:
                return "Creature Arrives";
            case TriggerCondition.ON_SELF_DAMAGE_DEALT:
                return "Upon dealing damage";
            case TriggerCondition.ON_SELF_DAMAGE_TAKEN:
                return "Upon taking damage";
            case TriggerCondition.ON_SELF_DIES:
                return "Departure";
            case TriggerCondition.ON_CREATURE_DIES:
                return "Creatures Departs";
            case TriggerCondition.ON_STACK_UPDATED:
                return "Response";
            case TriggerCondition.IS_ALIVE:
                return "Static";
        }
        return PARSE_ERROR;
    }

    static public string Parse(CreatureType c)
    {
        switch (c)
        {
            case CreatureType.GOBLIN:
                return "Goblin";
            case CreatureType.HUMAN:
                return "Human";
        }
        return PARSE_ERROR;
    }

    static public string Parse(CardType c)
    {
        switch (c)
        {
            case CardType.CREATURE:
                return "Unit";
            case CardType.SPELL:
                return "Spell";
            case CardType.TRAP:
                return "Trap";
        }
        return PARSE_ERROR;
    }
}
