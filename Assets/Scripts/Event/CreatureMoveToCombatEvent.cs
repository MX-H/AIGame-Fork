using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class CreatureMoveToCombatEvent : IEvent
{
    public NetworkIdentity creatureId;
    public int arenaPosition;

    public CreatureMoveToCombatEvent()
    {
    }

    public CreatureMoveToCombatEvent(PlayerController player, Creature creature, int position) : base(player)
    {
        creatureId = creature.netIdentity;
        arenaPosition = position;
    }
}
