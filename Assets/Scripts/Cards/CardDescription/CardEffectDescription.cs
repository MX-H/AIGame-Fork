using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardEffectDescription
{
    public TriggerCondition triggerCondition;
    public TargetType targetType;
    public TargettingType targettingType;
    public IEffectDescription effectType;

    public string CardText()
    {
        return ((triggerCondition == TriggerCondition.NONE) ? "" : CardParsing.Parse(triggerCondition) + ": ")
            + TargetsToString() + " " + effectType.CardText();
    }

    private string TargetsToString()
    {
        string output = "";
        switch (targettingType)
        {
            case TargettingType.SELF:
                return "you";
            case TargettingType.FOE:
                return "opponent";
            case TargettingType.TARGET:
                output += "target ";
                break;
            case TargettingType.ALL:
                output += "all ";
                break;
            case TargettingType.UP_TO_TARGET:
                output += "up to X ";
                break;
            case TargettingType.EXCEPT:
                output += "all ";
                break;
        }

        switch (targetType)
        {
            case TargetType.CREATURES:
                output += "creature(s)";
                break;
            case TargetType.SET_TRAPS:
                output += "set trap(s)";
                break;
            case TargetType.CARDS:
                output += "card(s)";
                break;
            case TargetType.PLAYERS:
                output += "player(s)";
                break;
            case TargetType.CREATURE_CARDS:
                output += "creature card(s)";
                break;
            case TargetType.SPELL_CARDS:
                output += "spell card(s)";
                break;
            case TargetType.TRAP_CARDS:
                output += "trap card(s)";
                break;
            case TargetType.PERMANENT:
                output += "permanent(s)";
                break;
            case TargetType.DAMAGEABLE:
                output += "creature(s) or player(s)";
                break;
            case TargetType.ACTIVE_TRAPS:
                output += "trap(s)";
                break;
            case TargetType.SPELLS:
                output += "spell(s)";
                break;
            case TargetType.EFFECTS:
                output += "effect(s)";
                break;
            case TargetType.SPELLS_AND_TRAPS:
                output += "spell(s) or trap(s)";
                break;
            case TargetType.SPELLS_AND_EFFECTS:
                output += "spell(s) or effect(s)";
                break;
            case TargetType.TRAPS_AND_EFFECTS:
                output += "trap(s) or effect(s)";
                break;
            case TargetType.STACK_ITEMS:
                output += "spell(s), trap(s), or effect(s)";
                break;
        }
        return output;
    }
}
