using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfirmationEvent : IEvent
{
    public ConfirmationEvent()
    { }
    public ConfirmationEvent(PlayerController player) : base(player)
    { }
}
