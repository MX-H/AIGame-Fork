using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CancelPlayCardEvent : IEvent
{
    public CancelPlayCardEvent()
    { }
    public CancelPlayCardEvent(PlayerController player) : base(player)
    { }
}
