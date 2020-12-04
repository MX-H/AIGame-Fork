using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static public class CardParsing
{
    static private readonly string PARSE_ERROR = "MISSING STRING";

    static public string CapitalizeSentence(string s)
    {
        if (s.Length > 0)
        {
            s = s[0].ToString().ToUpper() + s.Substring(1);
        }
        return s;
    }
    static public string Parse(KeywordAttribute a)
    {
        switch (a)
        {
            case KeywordAttribute.NONE:
                return "<NONE>";
            case KeywordAttribute.EVASION:
                return "Evasive";
            case KeywordAttribute.FAST_STRIKE:
                return "Deft";
            case KeywordAttribute.PIERCING:
                return "Excessive";
            case KeywordAttribute.UNTOUCHABLE:
                return "Untouchable";
            case KeywordAttribute.EAGER:
                return "Eager";
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
            case TriggerCondition.ON_SELF_DAMAGE_DEALT_TO_PLAYER:
                return "Breach";
            case TriggerCondition.ON_SELF_DAMAGE_TAKEN:
                return "Endure";
            case TriggerCondition.ON_SELF_DIES:
                return "Departure";
            case TriggerCondition.ON_CREATURE_DIES:
                return "Creature Departs";
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
            case CreatureType.FAERIE:
                return "Faerie";
        }
        return PARSE_ERROR;
    }

    static public string Parse(CardType c)
    {
        switch (c)
        {
            case CardType.CREATURE:
                return "Creature";
            case CardType.SPELL:
                return "Spell";
            case CardType.TRAP:
                return "Ambush";
        }
        return PARSE_ERROR;
    }

    static public string Parse(TargetType t, bool plural)
    {
        switch (t)
        {
            case TargetType.CREATURES:
                return plural ? "creatures" : "creature";
            case TargetType.SET_TRAPS:
                return plural ? "set traps" : "set trap";
            case TargetType.CARDS:
                return plural ? "cards" : "card";
            case TargetType.PLAYERS:
                return plural ? "players" : "player";
            case TargetType.CREATURE_CARDS:
                return plural ? "creature cards" : "creature card";
            case TargetType.SPELL_CARDS:
                return plural ? "spell cards" : "spell card";
            case TargetType.TRAP_CARDS:
                return plural ? "ambush cards" : "ambush card";
            case TargetType.PERMANENT:
                return plural ? "permanents" : "permanent";
            case TargetType.DAMAGEABLE:
                return Parse(TargetType.CREATURES, plural) + " or " + Parse(TargetType.PLAYERS, plural);
            case TargetType.ACTIVE_TRAPS:
                return plural ? "ambushes" : "ambush";
            case TargetType.SPELLS:
                return plural ? "spells" : "spell";
            case TargetType.EFFECTS:
                return plural ? "effects" : "effect";
            case TargetType.SPELLS_AND_TRAPS:
                return Parse(TargetType.SPELLS, plural) + " or " + Parse(TargetType.ACTIVE_TRAPS, plural);
            case TargetType.SPELLS_AND_EFFECTS:
                return Parse(TargetType.SPELLS, plural) + " or " + Parse(TargetType.EFFECTS, plural);
            case TargetType.TRAPS_AND_EFFECTS:
                return Parse(TargetType.ACTIVE_TRAPS, plural) + " or " + Parse(TargetType.EFFECTS, plural);
            case TargetType.STACK_ITEMS:
                return Parse(TargetType.SPELLS, plural) + ", " + Parse(TargetType.ACTIVE_TRAPS, plural) + ", or " + Parse(TargetType.PLAYERS, plural);
        }
        return PARSE_ERROR;
    }
}
