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
        foreach (Trap t in traps)
        {
            t.owner = p;
        }

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
        GameSession gameSession = GameUtils.GetGameSession();
        if (gameSession)
        {
            if (gameSession.GetCurrState() == GameSession.GameState.RESOLVING_COMBAT)
            {
                float step = 10f * Time.deltaTime;
                foreach (Creature attacker in incomingAttackers)
                {
                    Transform creatureTransform = attacker.transform;
                    creatureTransform.position = Vector3.MoveTowards(creatureTransform.position, new Vector3(creatureTransform.position.x, (0 - creatureTransform.position.y) / 2, creatureTransform.position.z), step);
                }
            }
            else
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

    public bool IsValidBlock(Creature creature, int ind)
    {
        if (incomingAttackers != null && ind < incomingAttackers.Count)
        {
            if (!incomingAttackers[ind].HasKeyword(KeywordAttribute.EVASION) || creature.HasKeyword(KeywordAttribute.EVASION))
            {
                return true;
            }
        }
        return false;
    }

    public void AddCreature(Creature c)
    {
        if (creatures.Count < GameConstants.MAX_CREATURES)
        {
            creatures.Add(c);
            c.transform.SetParent(creatureZones.transform);
            c.transform.position = new Vector3(0, 0, 0);
            c.context = this;

            GameSession gameSession = GameUtils.GetGameSession();
            if (gameSession.isServer)
            {
                gameSession.ApplyStaticEffectsToTargettable(c);
                gameSession.ServerAddStaticEffects(c);
            }
        }
    }

    public void RemoveCreature(Creature c)
    {
        inCombatCreatures.Remove(c);
        creatures.Remove(c);
        c.context = null;
        c.gameObject.SetActive(false);

        foreach (Targettable target in GameUtils.GetGameSession().GetPotentialTargets())
        {
            if (target is Creature targetCreature)
            {
                targetCreature.GetCreatureState().RemoveAuraModifiers(c);
            }
            else if (target is Card targetCard)
            {
                targetCard.cardData.RemoveAuraModifiers(c);
            }
        }
    }

    public void DropCreature(Creature creature)
    {
        if (!player.CanMoveCreatures())
        {
            return;
        }

        Camera cam = Camera.main;

        float heightVal = cam.WorldToScreenPoint(creature.transform.position).y;
        float creatureDiff = Mathf.Abs(heightVal - cam.WorldToScreenPoint(creatureZones.transform.position).y);
        float combatDiff = Mathf.Abs(heightVal - cam.WorldToScreenPoint(combatZones.transform.position).y);

        // Trying to drop into creatures row
        if (creatureDiff < combatDiff)
        {
            if (!creatures.Contains(creature))
            {
                player.ClientRequestRemoveFromCombat(creature);
            }
        }
        // Trying to drop into combat row
        else
        {
            float xVal = cam.WorldToScreenPoint(creature.transform.position).x;
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
                    player.ClientRequestMoveToCombat(creature, closestInd);
                }
            }
            else if (!creature.GetCreatureState().IsSummoningSick())
            {
                int ind = 0;
                for (; ind < inCombatCreatures.Count; ind++)
                {
                    if (!inCombatCreatures[ind].dragging && xVal < cam.WorldToScreenPoint(inCombatCreatures[ind].transform.position).x)
                    {
                        break;
                    }
                }

                player.ClientRequestMoveToCombat(creature, ind);
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

    public void AddTrap(Card c, int index)
    {
        if (!traps[index].IsActive())
        {
            traps[index].SetCard(c);
        }
    }

    public int GetTrapIndex(Card c)
    {
        for (int i = 0; i < traps.Length; i++)
        {
            if (traps[i].IsActive() && traps[i].card == c)
            {
                return i;
            }
        }
        return -1;
    }

    public bool IsTrap(Card c)
    {
        return GetTrapIndex(c) >= 0;
    }

    public void RemoveTrap(Card c)
    {
        foreach (Trap t in traps)
        {
            if (t.IsActive() && t.card == c)
            {
                t.SetCard(null);
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

    public List<Creature> GetAllCreatures()
    {
        List<Creature> creatureList = new List<Creature>();
        creatureList.AddRange(inCombatCreatures);
        creatureList.AddRange(creatures);
        return creatureList;
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
    public bool IsInCombat()
    {
        return arenaState != State.NONE;
    }

    public List<Targettable> GetTargettables()
    {
        List<Targettable> targets = new List<Targettable>();
        foreach (Trap t in traps)
        {
            if (t.IsActive())
            {
                targets.Add(t);
            }
        }

        targets.AddRange(creatures);
        targets.AddRange(inCombatCreatures);

        return targets;
    }
}
