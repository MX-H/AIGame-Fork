using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public abstract class IModifier
{
    public DurationType modifierDuration;
    public NetworkIdentity auraSource; // Used for aura durations

    public IModifier()
    {
        modifierDuration = DurationType.FOREVER;
    }
    public IModifier(DurationType duration)
    {
        modifierDuration = duration;
    }

    public IModifier(Creature source)
    {
        modifierDuration = DurationType.AURA;
        auraSource = source.netIdentity;
    }
}
