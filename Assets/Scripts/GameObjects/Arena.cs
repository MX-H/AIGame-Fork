using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Arena : MonoBehaviour
{
    public enum State
    {
        NONE, ATTACKING, BLOCKING
    }

    // Start is called before the first frame update
    PlayerController player;
    List<Creature> creatures;
    List<Creature> inCombatCreatures;

    List<Creature> incomingAttackers;
    Creature[] defenders;

    public Trap[] traps = new Trap[GameConstants.MAX_TRAPS];
    State arenaState;
    public GameObject creatureZones;
    public GameObject combatZones;


    public void AssignPlayer(PlayerController p)
    {
        player = p;

        p.arena = this;
    }

    void Start()
    {
        creatures = new List<Creature>();
        inCombatCreatures = new List<Creature>();

        foreach (Creature c in creatureZones.GetComponentsInChildren<Creature>())
        {
            creatures.Add(c);
        }
        foreach (Creature c in combatZones.GetComponentsInChildren<Creature>())
        {
            inCombatCreatures.Add(c);
        }
    }

    // Update is called once per frame
    void Update()
    {
        float center = (creatures.Count - 1) / 2.0f;
        for (int i = 0; i < creatures.Count; i++)
        {
            if (!creatures[i].dragging)
            {
                creatures[i].transform.localPosition = new Vector3((i - center) * 5.0f, 0, 0);
                creatures[i].transform.localRotation = Quaternion.identity;
            }
        }

        if (arenaState == State.ATTACKING)
        {
            int onField = 0;
            foreach (Creature c in inCombatCreatures)
            {
                if (!c.dragging)
                {
                    onField++;
                }
            }

            if (onField > 0)
            {
                int ind = 0;
                center = (onField - 1) / 2.0f;
                for (int i = 0; i < inCombatCreatures.Count; i++)
                {
                    if (!inCombatCreatures[i].dragging)
                    {
                        inCombatCreatures[i].transform.localPosition = new Vector3((ind - center) * 5.0f, 0, 0);
                        inCombatCreatures[i].transform.localRotation = Quaternion.identity;
                        ind++;
                    }
                }
            }
        }
        else if (arenaState == State.BLOCKING)
        {
            center = (incomingAttackers.Count - 1) / 2.0f;
            for (int i = 0; i < defenders.Length; i++)
            {
                if (defenders[i] != null && !defenders[i].dragging)
                {
                    defenders[i].transform.localPosition = new Vector3((i - center) * 5.0f, 0, 0);
                    defenders[i].transform.localRotation = Quaternion.identity;
                }
            }
        }

    }

    public void SetState(State state)
    {
        arenaState = state;
        switch (state)
        {
            case State.NONE:
                while (inCombatCreatures.Count > 0)
                {
                    RemoveFromCombat(inCombatCreatures[0]);
                }
                defenders = null;
                incomingAttackers = null;
                break;
            default:
                break;
        }
    }

    public State GetState()
    {
        return arenaState;
    }

    public void AddCreature(Creature c)
    {
        if (creatures.Count < GameConstants.MAX_CREATURES)
        {
            creatures.Add(c);
            c.transform.SetParent(creatureZones.transform);
            c.transform.position = new Vector3(0, 0, 0);
            c.context = this;
        }
    }

    public void RemoveCreature(Creature c)
    {
        inCombatCreatures.Remove(c);
        creatures.Remove(c);
        c.context = null;
        c.gameObject.SetActive(false);
    }

    public void DropCreature(Creature c)
    {
        if (!player.CanMoveCreatures())
        {
            return;
        }

        Camera cam = Camera.main;

        float heightVal = cam.WorldToScreenPoint(c.transform.position).y;
        float creatureDiff = Mathf.Abs(heightVal - cam.WorldToScreenPoint(creatureZones.transform.position).y);
        float combatDiff = Mathf.Abs(heightVal - cam.WorldToScreenPoint(combatZones.transform.position).y);

        // Trying to drop into creatures row
        if (creatureDiff < combatDiff)
        {
            if (!creatures.Contains(c))
            {
                player.ClientRequestRemoveFromCombat(c.gameObject.GetComponent<NetworkIdentity>());
            }
        }
        // Trying to drop into combat row
        else
        {
            float xVal = cam.WorldToScreenPoint(c.transform.position).x;
            // Compare x values to get position in combat row

            if (arenaState == State.BLOCKING)
            {
                float smallestXDiff = -1;
                int closestInd = 0;

                for (int i = 0; i < incomingAttackers.Count; i++)
                {
                    float xDiff = Mathf.Abs(xVal - cam.WorldToScreenPoint(incomingAttackers[i].transform.position).x);
                    if (smallestXDiff < 0 || xDiff < smallestXDiff)
                    {
                        smallestXDiff = xDiff;
                        closestInd = i;
                    }
                }

                if (defenders[closestInd] == null)
                {
                    player.ClientRequestMoveToCombat(c.gameObject.GetComponent<NetworkIdentity>(), closestInd);
                }
            }
            else
            {
                int ind = 0;
                for (; ind < inCombatCreatures.Count; ind++)
                {
                    if (!inCombatCreatures[ind].dragging && xVal < cam.WorldToScreenPoint(inCombatCreatures[ind].transform.position).x)
                    {
                        break;
                    }
                }

                player.ClientRequestMoveToCombat(c.gameObject.GetComponent<NetworkIdentity>(), ind);
            }
        }
    }

    public void AddTrap(Card c)
    {
        foreach (Trap t in traps)
        {
            if (!t.IsActive())
            {
                t.SetCard(c);
                return;
            }
        }
    }

    public bool HasSpaceTrap()
    {
        foreach (Trap t in traps)
        {
            if (!t.IsActive())
            {
                return true;
            }
        }
        return false;
    }

    public bool HasSpaceCreature()
    {
        return creatures.Count < GameConstants.MAX_CREATURES;
    }

    public void MoveToCombat(Creature c, int position)
    {
        if (creatures.Contains(c) && inCombatCreatures.Count < GameConstants.MAX_CREATURES && arenaState != State.NONE)
        {
            creatures.Remove(c);
            c.transform.SetParent(combatZones.transform);
        }
        else if (inCombatCreatures.Contains(c))
        {
            inCombatCreatures.Remove(c);
            for (int i = 0; i < defenders.Length; i++)
            {
                if (defenders[i] == c)
                {
                    defenders[i] = null;
                }
            }
        }

        if (arenaState == State.BLOCKING)
        {
            inCombatCreatures.Add(c);
            defenders[position] = c;
        }
        else
        {
            inCombatCreatures.Insert(position, c);
        }
    }

    public void RemoveFromCombat(Creature c)
    {
        if (inCombatCreatures.Contains(c))
        {
            inCombatCreatures.Remove(c);
            if (arenaState == State.BLOCKING)
            {
                for (int i = 0; i < defenders.Length; i++)
                {
                    if (defenders[i] == c)
                    {
                        defenders[i] = null;
                    }
                }
            }

            if (creatures.Count < GameConstants.MAX_CREATURES)
            {
                creatures.Add(c);
                c.transform.SetParent(creatureZones.transform);
            }
            else
            {
                RemoveCreature(c);
            }
        }
    }

    public List<Creature> GetCombatCreatures()
    {
        return inCombatCreatures;
    }

    public List<Creature> GetStandbyCreatures()
    {
        return creatures;
    }

    public Creature[] GetDefenders()
    {
        return defenders;
    }

    public void ReceiveAttackers(List<Creature> attackers)
    {
        incomingAttackers = attackers;
        defenders = new Creature[attackers.Count];
    }

    public void Reset()
    {
        foreach (Creature c in creatures)
        {
            Destroy(c.gameObject);
        }
        creatures.Clear();

        foreach (Creature c in inCombatCreatures)
        {
            Destroy(c.gameObject);
        }
        inCombatCreatures.Clear();
    }

    public int OnStandByCount()
    {
        return creatures.Count;
    }

    public int InCombatCount()
    {
        return inCombatCreatures.Count;
    }
}
