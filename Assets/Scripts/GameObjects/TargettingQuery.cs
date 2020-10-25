using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class TargettingQuery
{
    public TargettingQuery(ITargettingDescription desc, PlayerController player)
    {
        targettingDesc = desc;
        requestingPlayer = player;
    }

    public ITargettingDescription targettingDesc;
    public PlayerController requestingPlayer;
}
