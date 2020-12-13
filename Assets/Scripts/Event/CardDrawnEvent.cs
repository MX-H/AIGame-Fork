using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class CardDrawnEvent : IEvent
{
    public NetworkIdentity cardId;
    public CardDrawnEvent()
    { }

    public CardDrawnEvent(PlayerController player, Card card) : base(player)
    {
        cardId = card.netIdentity;
    }
}
