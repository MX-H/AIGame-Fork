using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class TargetSelectionEvent : IEvent
{
    public NetworkIdentity[] flattenedTargets;
    public int[] indexes;

    public TargetSelectionEvent()
    {
    }

    public TargetSelectionEvent(PlayerController player, List<List<Targettable>> targets) : base(player)
    {
        indexes = new int[targets.Count];
        int totalSize = 0;
        for (int i = 0; i < targets.Count; i++)
        {
            totalSize += targets[i].Count;
            indexes[i] = targets[i].Count;
        }

        flattenedTargets = new NetworkIdentity[totalSize];
        int ind = 0;
        for (int i = 0; i < targets.Count; i++)
        {
            for (int j = 0; j < targets[i].Count; j++, ind++)
            {
                flattenedTargets[ind] = targets[i][j].GetComponent<NetworkIdentity>();
            }
        }
    }

    public NetworkIdentity[][] ReconstructTargets()
    {
        NetworkIdentity[][] targets = new NetworkIdentity[indexes.Length][];
        int ind = 0;
        for (int i = 0; i < indexes.Length; i++)
        {
            targets[i] = new NetworkIdentity[indexes[i]];
            for (int j = 0; j < indexes[i]; j++, ind++)
            {
                targets[i][j] = flattenedTargets[ind];
            }
        }
        return targets;
    }
}
