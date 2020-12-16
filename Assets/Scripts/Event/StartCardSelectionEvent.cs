using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class StartCardSelectionEvent : IEvent
{
    public NetworkIdentity srcPlayer;
    public int seed1;
    public int seed2;
    public int seed3;
    public CardGenerationFlags flags;
    public StartCardSelectionEvent()
    { }

    public StartCardSelectionEvent(PlayerController player, PlayerController source, int cardSeed1, int cardSeed2, int cardSeed3, CardGenerationFlags cardFlags) : base(player)
    {
        srcPlayer = source.netIdentity;
        seed1 = cardSeed1;
        seed2 = cardSeed2;
        seed3 = cardSeed3;
        flags = cardFlags;
    }
}
