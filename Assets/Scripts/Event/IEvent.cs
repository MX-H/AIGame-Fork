using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public abstract class IEvent
{
    public IEvent()
    {
        playerId = null;
    }
    public IEvent(PlayerController playerSender)
    {
        playerId = playerSender.netIdentity;
    }

    public NetworkIdentity playerId;
}
