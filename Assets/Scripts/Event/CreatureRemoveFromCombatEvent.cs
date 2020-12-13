using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class CreatureRemoveFromCombatEvent : IEvent
{
    public NetworkIdentity creatureId;

    public CreatureRemoveFromCombatEvent()
    { }
    public CreatureRemoveFromCombatEvent(PlayerController player, Creature creature) : base(player)
    {
        creatureId = creature.netIdentity;
    }

}
