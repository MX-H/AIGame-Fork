using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class UseTrapEvent : IEvent
{
    public NetworkIdentity cardId;

    public NetworkIdentity[] flattenedTargets;
    public int[] indexes;

    // Start is called before the first frame update
    public UseTrapEvent()
    { }

    public UseTrapEvent(PlayerController player, Card card) : base(player)
    {
        cardId = card.netIdentity;
        flattenedTargets = null;
        indexes = null;
    }

    public UseTrapEvent(PlayerController player, Card card, List<List<Targettable>> targets) : base(player)
    {
        cardId = card.netIdentity;
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

    public UseTrapEvent(PlayerController player, Card card, NetworkIdentity[] targets, int[] indices) : base(player)
    {
        cardId = card.netIdentity;
        flattenedTargets = targets;
        indexes = indices;
    }
}
