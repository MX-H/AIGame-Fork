using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class TargettingQuery
{
    public TargettingQuery(ITargettingDescription desc, PlayerController player, bool targets = true)
    {
        targettingDesc = desc;
        requestingPlayer = player;
        ignoreUntouchable = !targets;
    }

    public ITargettingDescription targettingDesc;
    public PlayerController requestingPlayer;
    public bool ignoreUntouchable;
}
