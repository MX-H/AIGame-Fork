using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class CardSelectionEvent : IEvent
{
    public NetworkIdentity srcPlayer;
    public int seed;
    public CardGenerationFlags flags;

    public CardSelectionEvent()
    { }

    public CardSelectionEvent(PlayerController player, PlayerController source, int cardSeed, CardGenerationFlags cardFlags) : base(player)
    {
        srcPlayer = source.netIdentity;
        seed = cardSeed;
        flags = cardFlags;
    }

}
